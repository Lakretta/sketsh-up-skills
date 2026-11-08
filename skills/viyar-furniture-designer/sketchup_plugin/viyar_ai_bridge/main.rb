# encoding: UTF-8
# frozen_string_literal: false

require 'sketchup.rb'
require 'socket'
require 'json'

module ViyarAIBridge
  PORT = 9876
  HOST = '127.0.0.1'.freeze
  DEFAULT_WORKSPACE = 'D:/Project/Antigravity/Scratch/viyar-furniture-agent'.freeze
  LIVE_QUEUE_FILE   = File.join(DEFAULT_WORKSPACE, 'output', 'sketchup_live_queue.json').freeze

  @server = nil
  @timer_id = nil
  @clients = []
  @last_queue_mtime = nil

  class << self
    attr_accessor :server, :timer_id

    # =========================================================================
    # 1. TCP SERVER & LIVE QUEUE BRIDGE FOR AI AGENT
    # =========================================================================
    def start_server
      return if @server

      begin
        @server = TCPServer.new(HOST, PORT)
        @clients = []
        @timer_id = UI.start_timer(0.15, true) { poll_events }
        Sketchup.status_text = "🟢 Viyar AI Agent Bridge активний на #{HOST}:#{PORT}"
        puts "[ViyarAIBridge] TCP Server started on #{HOST}:#{PORT}"
      rescue StandardError => e
        UI.messagebox("Не вдалося запустити Viyar AI TCP сервер на порту #{PORT}: #{e.message}")
      end
    end

    def stop_server
      if @timer_id
        UI.stop_timer(@timer_id)
        @timer_id = nil
      end
      @clients.each do |c|
        c.close rescue nil
      end
      @clients.clear
      if @server
        @server.close rescue nil
        @server = nil
      end
      Sketchup.status_text = "🔴 Viyar AI Agent Bridge зупинено"
      puts "[ViyarAIBridge] TCP Server stopped"
    end

    def poll_events
      poll_tcp_clients if @server
      poll_live_queue_file
    rescue StandardError => e
      puts "[ViyarAIBridge] Poll error: #{e.message}"
    end

    def poll_tcp_clients
      loop do
        client = @server.accept_nonblock
        @clients << client
      rescue IO::WaitReadable, Errno::EWOULDBLOCK
        break
      rescue StandardError
        break
      end

      @clients.dup.each do |client|
        begin
          raw = client.read_nonblock(65536)
          if raw && !raw.strip.empty?
            raw.each_line do |line|
              next if line.strip.empty?
              response = handle_json_command(line.strip)
              client.write(response.to_json + "\n")
              client.flush
            end
          end
        rescue IO::WaitReadable, Errno::EWOULDBLOCK
          # No data yet
        rescue EOFError, StandardError => e
          @clients.delete(client)
          client.close rescue nil
        end
      end
    end

    def poll_live_queue_file
      return unless File.exist?(LIVE_QUEUE_FILE)

      mtime = File.mtime(LIVE_QUEUE_FILE)
      return if @last_queue_mtime && mtime <= @last_queue_mtime

      @last_queue_mtime = mtime
      content = File.read(LIVE_QUEUE_FILE, encoding: 'UTF-8')
      return if content.strip.empty?

      data = JSON.parse(content)
      return if data['processed'] == true

      result = dispatch_command(data['command'] || 'clear_and_build', data['params'] || data)
      data['processed'] = true
      data['processed_at'] = Time.now.iso8601 rescue Time.now.to_s
      data['result'] = result
      File.write(LIVE_QUEUE_FILE, JSON.pretty_generate(data), encoding: 'UTF-8')
      @last_queue_mtime = File.mtime(LIVE_QUEUE_FILE)
    rescue StandardError => e
      puts "[ViyarAIBridge] Queue file error: #{e.message}"
    end

    def handle_json_command(json_str)
      req = JSON.parse(json_str)
      cmd = req['command'] || req['method'] || 'ping'
      params = req['params'] || req['arguments'] || req
      id = req['id']
      res = dispatch_command(cmd, params)
      { jsonrpc: '2.0', id: id, status: 'ok', result: res }
    rescue StandardError => e
      { jsonrpc: '2.0', id: (req ? req['id'] : nil), status: 'error', error: { message: e.message, backtrace: e.backtrace.first(5) } }
    end

    def dispatch_command(cmd, params)
      case cmd.to_s
      when 'ping'
        {
          sketchup_version: Sketchup.version,
          plugin_version: PLUGIN_VERSION,
          active_model: Sketchup.active_model ? Sketchup.active_model.title : '',
          entities_count: Sketchup.active_model ? Sketchup.active_model.active_entities.length : 0
        }
      when 'build_project'
        build_project_from_data(params)
      when 'clear_and_build'
        clear_viyar_entities
        build_project_from_data(params)
      when 'inspect_scene'
        inspect_active_scene
      when 'export_scene_to_viyar'
        out_dir = params['output_dir'] || File.join(DEFAULT_WORKSPACE, 'output')
        export_active_scene_to_viyar(out_dir)
      when 'eval_ruby'
        code = params['code'].to_s
        res = eval(code) # rubocop:disable Security/Eval
        { evaluated: true, output: res.to_s }
      else
        raise "Невідома команда: #{cmd}"
      end
    end

    # =========================================================================
    # 2. 3D FURNITURE MODEL BUILDER WITH ABF / GAVA / OPENCUTLIST / DC TAGS
    # =========================================================================
    def clear_viyar_entities
      model = Sketchup.active_model
      return unless model

      model.start_operation('AI Agent: Очищення моделі ВіЯр', true)
      to_erase = model.active_entities.select do |e|
        (e.is_a?(Sketchup::Group) || e.is_a?(Sketchup::ComponentInstance)) &&
          (e.get_attribute('ViyarPro', 'is_viyar_entity') || e.get_attribute('ABF_Data', 'is_panel'))
      end
      model.active_entities.erase_entities(to_erase) unless to_erase.empty?
      model.commit_operation
    end

    def ensure_layer(model, name, visible = true, color_rgb = nil)
      layer = model.layers[name] || model.layers.add(name)
      layer.visible = visible
      if color_rgb && layer.respond_to?(:color=)
        layer.color = Sketchup::Color.new(*color_rgb)
      end
      layer
    end

    def ensure_material(model, mat_info)
      mat_name = mat_info['name'] || mat_info['material_name'] || 'ЛДСП Egger W1000 18мм'
      mat = model.materials[mat_name] || model.materials.add(mat_name)
      hex = (mat_info['color_hex'] || '#F2F3F5').sub('#', '')
      if hex.length == 6
        r = hex[0..1].to_i(16)
        g = hex[2..3].to_i(16)
        b = hex[4..5].to_i(16)
        mat.color = Sketchup::Color.new(r, g, b)
      end

      thickness_mm = (mat_info['thickness_mm'] || 18.0).to_f
      sheet_l = (mat_info['sheet_length_mm'] || 2800).to_i
      sheet_w = (mat_info['sheet_width_mm'] || 2070).to_i
      grained = mat_info['has_grain'] ? true : false

      # OpenCutList compatibility attributes on Sketchup::Material
      mat.set_attribute('ladb_opencutlist', 'type', 2) # 2 = Sheet Good (Плитний матеріал)
      mat.set_attribute('ladb_opencutlist', 'thickness', "#{thickness_mm.round(1)}mm")
      mat.set_attribute('ladb_opencutlist', 'std_thicknesses', "#{thickness_mm.round(1)}mm")
      mat.set_attribute('ladb_opencutlist', 'std_dimension', "#{sheet_l}mm x #{sheet_w}mm")
      mat.set_attribute('ladb_opencutlist', 'grained', grained)

      # ViyarPro / Gava material attributes
      mat.set_attribute('ViyarPro', 'viyar_article', (mat_info['viyar_article'] || '').to_s)
      mat.set_attribute('ViyarPro', 'thickness_mm', thickness_mm)
      mat.set_attribute('Gava', 'article', (mat_info['viyar_article'] || '').to_s)
      mat.set_attribute('Gava', 'thickness', thickness_mm)
      mat
    end

    def build_project_from_data(project)
      model = Sketchup.active_model
      raise 'Немає активної моделі SketchUp' unless model

      model.start_operation("AI Agent: Проект #{project['project_name'] || 'Меблі ВіЯр'}", true)

      layer_body   = ensure_layer(model, 'Viyar_Корпус', true, [220, 225, 230])
      layer_facade = ensure_layer(model, 'Viyar_Фасади', true, [100, 140, 180])
      layer_hdf    = ensure_layer(model, 'Viyar_ХДФ_Задня_стінка', true, [200, 190, 170])
      layer_edge   = ensure_layer(model, 'Viyar_Крайка_Візуалізація', true, [255, 90, 40])
      layer_drill  = ensure_layer(model, 'ABF_drill', true, [255, 0, 0])
      layer_groove = ensure_layer(model, 'ABF_groove', true, [0, 170, 255])

      edge_mat_2mm = model.materials['Viyar_Крайка_2.0мм'] || model.materials.add('Viyar_Крайка_2.0мм')
      edge_mat_2mm.color = Sketchup::Color.new(235, 85, 45)
      edge_mat_05mm = model.materials['Viyar_Крайка_0.5мм'] || model.materials.add('Viyar_Крайка_0.5мм')
      edge_mat_05mm.color = Sketchup::Color.new(50, 165, 230)

      materials_map = {}
      (project['materials_used'] || []).each do |m|
        materials_map[m['id']] = ensure_material(model, m)
      end

      project_group = model.active_entities.add_group
      project_group.name = project['project_name'] || 'Проект_ВіЯр'
      project_group.set_attribute('ViyarPro', 'is_viyar_entity', true)
      project_group.set_attribute('ViyarPro', 'project_name', project_group.name)

      panels_created = 0
      modules_list = project['modules'] || []
      all_panels = project['panels'] || []

      # Group panels by module_id if modules are present
      panels_by_module = Hash.new { |h, k| h[k] = [] }
      all_panels.each { |p| panels_by_module[p['module_id'] || 'default'] << p }

      if modules_list.empty?
        modules_list = [{ 'id' => 'default', 'name' => project_group.name, 'type' => 'custom', 'width_mm' => 600, 'height_mm' => 720, 'depth_mm' => 560 }]
      end

      modules_list.each do |mod|
        mod_group = project_group.entities.add_group
        mod_group.name = mod['name'] || 'Модуль'
        mod_group.set_attribute('ViyarPro', 'is_viyar_entity', true)
        mod_group.set_attribute('ViyarPro', 'module_id', mod['id'].to_s)
        mod_group.set_attribute('ViyarPro', 'module_type', mod['type'].to_s)

        # Dynamic Components attributes for parametric editing in SketchUp
        mod_group.set_attribute('dynamic_attributes', '_formatversion', 1.0)
        mod_group.set_attribute('dynamic_attributes', '_has_behaviors', 1.0)
        mod_group.set_attribute('dynamic_attributes', 'lenx', (mod['width_mm'] || 600).to_f / 25.4)
        mod_group.set_attribute('dynamic_attributes', 'leny', (mod['depth_mm'] || 560).to_f / 25.4)
        mod_group.set_attribute('dynamic_attributes', 'lenz', (mod['height_mm'] || 720).to_f / 25.4)

        mod_panels = panels_by_module[mod['id']] || []
        mod_panels.each do |p|
          build_single_panel(
            mod_group.entities,
            p,
            materials_map,
            {
              body: layer_body,
              facade: layer_facade,
              hdf: layer_hdf,
              edge: layer_edge,
              drill: layer_drill,
              groove: layer_groove
            },
            { edge_2mm: edge_mat_2mm, edge_05mm: edge_mat_05mm }
          )
          panels_created += 1
        end
      end

      model.commit_operation
      model.active_view.zoom_extents
      {
        created_panels: panels_created,
        modules_count: modules_list.length,
        project_name: project_group.name
      }
    rescue StandardError => e
      model.abort_operation if model
      raise e
    end

    def build_single_panel(parent_entities, p, materials_map, layers, edge_mats)
      x_mm  = (p['x_mm'] || 0.0).to_f
      y_mm  = (p['y_mm'] || 0.0).to_f
      z_mm  = (p['z_mm'] || 0.0).to_f
      dx_mm = (p['dx_mm'] || p['length_mm'] || 600.0).to_f
      dy_mm = (p['dy_mm'] || p['width_mm'] || 500.0).to_f
      dz_mm = (p['dz_mm'] || p['thickness_mm'] || 18.0).to_f

      part_group = parent_entities.add_group
      part_name = p['name'] || 'Деталь'
      part_id   = p['part_number'] || p['id'] || 1
      part_group.name = format('%02d_%s (%dx%dx%d)', part_id.to_i, part_name, p['length_mm'].to_i, p['width_mm'].to_i, p['thickness_mm'].to_i)

      # Assign layer by role
      role = (p['role'] || 'body').to_s
      part_group.layer = if role.include?('facade') || role.include?('door') || role.include?('drawer_front')
                           layers[:facade]
                         elsif role.include?('hdf') || role.include?('back') || (p['thickness_mm'].to_f <= 4.5)
                           layers[:hdf]
                         else
                           layers[:body]
                         end

      # Assign SketchUp material
      mat = materials_map[p['material_id']]
      part_group.material = mat if mat

      # Create 3D box in local coordinates (0..dx, 0..dy, 0..dz) and transform to (x, y, z)
      pts = [
        Geom::Point3d.new(0, 0, 0),
        Geom::Point3d.new(dx_mm.mm, 0, 0),
        Geom::Point3d.new(dx_mm.mm, dy_mm.mm, 0),
        Geom::Point3d.new(0, dy_mm.mm, 0)
      ]
      face = part_group.entities.add_face(pts)
      face.reverse! if face.normal.z < 0
      face.pushpull(dz_mm.mm)

      # Move to world/module coordinates
      tr = Geom::Transformation.translation(Geom::Vector3d.new(x_mm.mm, y_mm.mm, z_mm.mm))
      part_group.transform!(tr)

      # Edge banding metadata
      edges = p['edges'] || {}
      e_l1 = (edges['l1'] || edges['top'] || '').to_s
      e_l2 = (edges['l2'] || edges['bottom'] || '').to_s
      e_w1 = (edges['w1'] || edges['left'] || '').to_s
      e_w2 = (edges['w2'] || edges['right'] || '').to_s

      # 1. ViyarPro & Gava attributes
      part_group.set_attribute('ViyarPro', 'is_viyar_entity', true)
      part_group.set_attribute('ViyarPro', 'part_id', part_id)
      part_group.set_attribute('ViyarPro', 'name', part_name)
      part_group.set_attribute('ViyarPro', 'material_id', p['material_id'].to_s)
      part_group.set_attribute('ViyarPro', 'material_article', p['material_article'].to_s)
      part_group.set_attribute('ViyarPro', 'material_name', p['material_name'].to_s)
      part_group.set_attribute('ViyarPro', 'length_mm', p['length_mm'].to_f)
      part_group.set_attribute('ViyarPro', 'width_mm', p['width_mm'].to_f)
      part_group.set_attribute('ViyarPro', 'thickness_mm', p['thickness_mm'].to_f)
      part_group.set_attribute('ViyarPro', 'has_grain', p['has_grain'] ? true : false)
      part_group.set_attribute('ViyarPro', 'edge_l1', e_l1)
      part_group.set_attribute('ViyarPro', 'edge_l2', e_l2)
      part_group.set_attribute('ViyarPro', 'edge_w1', e_w1)
      part_group.set_attribute('ViyarPro', 'edge_w2', e_w2)
      part_group.set_attribute('ViyarPro', 'drilling_json', (p['drilling'] || []).to_json)
      part_group.set_attribute('ViyarPro', 'grooves_json', (p['grooves'] || []).to_json)

      part_group.set_attribute('Gava', 'is_panel', true)
      part_group.set_attribute('Gava', 'name', part_name)
      part_group.set_attribute('Gava', 'article', p['material_article'].to_s)
      part_group.set_attribute('Gava', 'length', p['length_mm'].to_f)
      part_group.set_attribute('Gava', 'width', p['width_mm'].to_f)
      part_group.set_attribute('Gava', 'thickness', p['thickness_mm'].to_f)
      part_group.set_attribute('Gava', 'edge_L1', e_l1)
      part_group.set_attribute('Gava', 'edge_L2', e_l2)
      part_group.set_attribute('Gava', 'edge_W1', e_w1)
      part_group.set_attribute('Gava', 'edge_W2', e_w2)

      # 2. ABF (AutoCAD Box Furniture) attributes
      part_group.set_attribute('ABF_Data', 'is_panel', true)
      part_group.set_attribute('ABF_Data', 'panel_name', part_name)
      part_group.set_attribute('ABF_Data', 'length_mm', p['length_mm'].to_f)
      part_group.set_attribute('ABF_Data', 'width_mm', p['width_mm'].to_f)
      part_group.set_attribute('ABF_Data', 'thickness_mm', p['thickness_mm'].to_f)
      part_group.set_attribute('ABF_Data', 'edge_l1', e_l1)
      part_group.set_attribute('ABF_Data', 'edge_l2', e_l2)
      part_group.set_attribute('ABF_Data', 'edge_w1', e_w1)
      part_group.set_attribute('ABF_Data', 'edge_w2', e_w2)

      # 3. Draw 3D CNC Drilling markers inside part_group on ABF_drill layer
      draw_panel_machining(part_group.entities, dx_mm, dy_mm, dz_mm, p, layers, edge_mats)
      part_group
    end

    def draw_panel_machining(entities, dx_mm, dy_mm, dz_mm, p, layers, _edge_mats)
      drills = p['drilling'] || []
      return if drills.empty? && (p['grooves'] || []).empty?

      drill_group = entities.add_group
      drill_group.name = 'ABF_Присадка'
      drill_group.layer = layers[:drill]

      # Determine how local panel coordinates (length_mm, width_mm, thickness_mm) map to 3D box (dx_mm, dy_mm, dz_mm)
      # Find which axis is thickness (smallest dimension matching thickness_mm)
      t_mm = (p['thickness_mm'] || 18.0).to_f
      l_mm = (p['length_mm'] || dx_mm).to_f

      drills.each do |d|
        next unless d['side'] == 'face_top' || d['side'] == 'face_bottom' || d['side'] == 'face'

        lx = d['x_mm'].to_f
        ly = d['y_mm'].to_f
        diam = (d['diameter_mm'] || 5.0).to_f
        radius = (diam / 2.0).mm
        next if radius <= 0

        # Map (lx, ly) onto the two main axes of the 3D bounding box
        center, normal = map_panel_2d_to_3d(lx, ly, dx_mm, dy_mm, dz_mm, t_mm, l_mm)
        drill_group.entities.add_circle(center, normal, radius, 16)
      end
    rescue StandardError
      # Ignore non-critical preview drawing issues
    end

    def map_panel_2d_to_3d(lx, ly, dx_mm, dy_mm, dz_mm, t_mm, l_mm)
      if (dz_mm - t_mm).abs < 1.0
        # Horizontal panel: thickness is Z
        if (dx_mm - l_mm).abs < 1.0
          [Geom::Point3d.new(lx.mm, ly.mm, dz_mm.mm), Geom::Vector3d.new(0, 0, 1)]
        else
          [Geom::Point3d.new(ly.mm, lx.mm, dz_mm.mm), Geom::Vector3d.new(0, 0, 1)]
        end
      elsif (dx_mm - t_mm).abs < 1.0
        # Vertical side panel: thickness is X
        if (dz_mm - l_mm).abs < 1.0
          [Geom::Point3d.new(dx_mm.mm, ly.mm, lx.mm), Geom::Vector3d.new(1, 0, 0)]
        else
          [Geom::Point3d.new(dx_mm.mm, lx.mm, ly.mm), Geom::Vector3d.new(1, 0, 0)]
        end
      else
        # Front/Back panel: thickness is Y
        if (dx_mm - l_mm).abs < 1.0
          [Geom::Point3d.new(lx.mm, dy_mm.mm, ly.mm), Geom::Vector3d.new(0, 1, 0)]
        else
          [Geom::Point3d.new(ly.mm, dy_mm.mm, lx.mm), Geom::Vector3d.new(0, 1, 0)]
        end
      end
    end

    # =========================================================================
    # 3. SCENE INSPECTOR & CONVERTER (READ ANY SKETCHUP MODEL -> VIYAR SPEC)
    # =========================================================================
    def inspect_active_scene
      model = Sketchup.active_model
      return { panels: [] } unless model

      panels = []
      collect_panels_recursive(model.active_entities, Geom::Transformation.new, panels)
      {
        model_title: model.title,
        panels_count: panels.length,
        panels: panels
      }
    end

    def collect_panels_recursive(entities, parent_tr, acc)
      entities.each do |ent|
        next unless ent.is_a?(Sketchup::Group) || ent.is_a?(Sketchup::ComponentInstance)
        next if ent.layer && (ent.layer.name == 'ABF_drill' || ent.layer.name == 'ABF_groove' || ent.layer.name == 'Viyar_Крайка_Візуалізація')

        tr = parent_tr * ent.transformation
        sub_entities = ent.is_a?(Sketchup::Group) ? ent.entities : ent.definition.entities

        # Check if this entity has explicit ViyarPro / Gava / ABF panel attributes
        if ent.get_attribute('ViyarPro', 'length_mm') || ent.get_attribute('Gava', 'is_panel') || ent.get_attribute('ABF_Data', 'is_panel')
          acc << extract_attributed_panel(ent, tr, acc.length + 1)
          next
        end

        # Check if it's a container assembly (has sub-groups besides machining helpers)
        child_groups = sub_entities.select do |c|
          (c.is_a?(Sketchup::Group) || c.is_a?(Sketchup::ComponentInstance)) &&
            c.name != 'ABF_Присадка'
        end

        if !child_groups.empty?
          collect_panels_recursive(sub_entities, tr, acc)
        else
          # Leaf solid group/component -> infer panel dimensions from local bounding box
          panel_info = infer_panel_from_bbox(ent, tr, acc.length + 1)
          acc << panel_info if panel_info
        end
      end
    end

    def extract_attributed_panel(ent, _tr, idx)
      length_mm    = (ent.get_attribute('ViyarPro', 'length_mm') || ent.get_attribute('Gava', 'length') || ent.get_attribute('ABF_Data', 'length_mm')).to_f
      width_mm     = (ent.get_attribute('ViyarPro', 'width_mm') || ent.get_attribute('Gava', 'width') || ent.get_attribute('ABF_Data', 'width_mm')).to_f
      thickness_mm = (ent.get_attribute('ViyarPro', 'thickness_mm') || ent.get_attribute('Gava', 'thickness') || ent.get_attribute('ABF_Data', 'thickness_mm')).to_f
      name         = (ent.get_attribute('ViyarPro', 'name') || ent.get_attribute('Gava', 'name') || ent.get_attribute('ABF_Data', 'panel_name') || ent.name).to_s
      mat_name     = (ent.get_attribute('ViyarPro', 'material_name') || (ent.material ? ent.material.display_name : 'ЛДСП 18мм')).to_s
      mat_article  = (ent.get_attribute('ViyarPro', 'material_article') || ent.get_attribute('Gava', 'article') || '').to_s

      {
        part_number: idx,
        name: name,
        length_mm: length_mm.round(1),
        width_mm: width_mm.round(1),
        thickness_mm: thickness_mm.round(1),
        material_name: mat_name,
        material_article: mat_article,
        has_grain: ent.get_attribute('ViyarPro', 'has_grain') ? true : false,
        edges: {
          l1: (ent.get_attribute('ViyarPro', 'edge_l1') || ent.get_attribute('Gava', 'edge_L1') || ent.get_attribute('ABF_Data', 'edge_l1') || '').to_s,
          l2: (ent.get_attribute('ViyarPro', 'edge_l2') || ent.get_attribute('Gava', 'edge_L2') || ent.get_attribute('ABF_Data', 'edge_l2') || '').to_s,
          w1: (ent.get_attribute('ViyarPro', 'edge_w1') || ent.get_attribute('Gava', 'edge_W1') || ent.get_attribute('ABF_Data', 'edge_w1') || '').to_s,
          w2: (ent.get_attribute('ViyarPro', 'edge_w2') || ent.get_attribute('Gava', 'edge_W2') || ent.get_attribute('ABF_Data', 'edge_w2') || '').to_s
        }
      }
    end

    def infer_panel_from_bbox(ent, _tr, idx)
      defn = ent.is_a?(Sketchup::Group) ? ent.entities.parent : ent.definition
      bb = defn.bounds
      return nil if bb.empty?

      dims = [bb.width.to_mm.round(1), bb.height.to_mm.round(1), bb.depth.to_mm.round(1)].sort
      thickness_mm = dims[0]
      width_mm     = dims[1]
      length_mm    = dims[2]

      # Filter out tiny hardware meshes or non-sheet objects
      return nil if thickness_mm < 2.0 || thickness_mm > 60.0 || length_mm < 40.0

      mat_name = ent.material ? ent.material.display_name : "Плитний матеріал #{thickness_mm.round}мм"
      {
        part_number: idx,
        name: ent.name.empty? ? "Деталь_#{idx}" : ent.name,
        length_mm: length_mm,
        width_mm: width_mm,
        thickness_mm: thickness_mm,
        material_name: mat_name,
        material_article: '',
        has_grain: false,
        edges: { l1: '0.5х22', l2: '0.5х22', w1: '0.5х22', w2: '0.5х22' }
      }
    end

    def export_active_scene_to_viyar(out_dir)
      scene_data = inspect_active_scene
      panels = scene_data[:panels]
      raise 'У поточній сцені SketchUp не знайдено деталей меблів' if panels.empty?

      Dir.mkdir(out_dir) unless Dir.exist?(out_dir)
      csv_path = File.join(out_dir, 'sketchup_scene_viyarpro.csv')
      xml_path = File.join(out_dir, 'sketchup_scene_viyarpro.xml')

      # Write ViyarPro CSV
      File.open(csv_path, 'w:UTF-8') do |f|
        f.write("\uFEFF") # UTF-8 BOM for Excel
        f.puts('№;Назва деталі;Довжина (мм);Ширина (мм);Товщина (мм);Кількість;Текстура;Матеріал;Артикул ВіЯр;Крайка L1 (Верх);Крайка L2 (Низ);Крайка W1 (Ліво);Крайка W2 (Право)')
        panels.each do |p|
          e = p[:edges] || {}
          grain = p[:has_grain] ? 'Так' : 'Ні'
          f.puts("#{p[:part_number]};#{p[:name]};#{p[:length_mm]};#{p[:width_mm]};#{p[:thickness_mm]};1;#{grain};#{p[:material_name]};#{p[:material_article]};#{e[:l1]};#{e[:l2]};#{e[:w1]};#{e[:w2]}")
        end
      end

      # Write ViyarPro XML
      File.open(xml_path, 'w:UTF-8') do |f|
        f.puts('<?xml version="1.0" encoding="UTF-8"?>')
        f.puts("<project name=\"#{scene_data[:model_title].empty? ? 'SketchUp_Viyar_Export' : scene_data[:model_title]}\" generator=\"ViyarAIBridge\">")
        f.puts('  <details>')
        panels.each do |p|
          e = p[:edges] || {}
          f.puts("    <detail id=\"#{p[:part_number]}\" name=\"#{p[:name]}\" length=\"#{p[:length_mm]}\" width=\"#{p[:width_mm]}\" thickness=\"#{p[:thickness_mm]}\" quantity=\"1\" texture=\"#{p[:has_grain] ? 1 : 0}\" material=\"#{p[:material_name]}\" article=\"#{p[:material_article]}\">")
          f.puts("      <edges l1=\"#{e[:l1]}\" l2=\"#{e[:l2]}\" w1=\"#{e[:w1]}\" w2=\"#{e[:w2]}\" />")
          f.puts('    </detail>')
        end
        f.puts('  </details>')
        f.puts('</project>')
      end

      { exported_panels: panels.length, csv_path: csv_path, xml_path: xml_path }
    end

    # =========================================================================
    # 4. SKETCHUP UI MENU & DIALOGS
    # =========================================================================
    def load_project_from_dialog
      path = UI.openpanel('Оберіть файл проекту Viyar AI Agent (.json)', File.join(DEFAULT_WORKSPACE, 'output'), 'JSON Files|*.json||')
      return unless path && File.exist?(path)

      data = JSON.parse(File.read(path, encoding: 'UTF-8'))
      res = build_project_from_data(data)
      UI.messagebox("✅ Побудовано проект '#{res[:project_name]}':\nМодулів: #{res[:modules_count]}\nДеталей з присадкою та крайкою: #{res[:created_panels]}")
    rescue StandardError => e
      UI.messagebox("Помилка завантаження проекту: #{e.message}")
    end

    def export_scene_from_menu
      out_dir = File.join(DEFAULT_WORKSPACE, 'output')
      res = export_active_scene_to_viyar(out_dir)
      UI.messagebox("✅ Експортовано #{res[:exported_panels]} деталей у ViyarPro!\n\nCSV: #{res[:csv_path]}\nXML: #{res[:xml_path]}")
    rescue StandardError => e
      UI.messagebox("Помилка експорту сцени: #{e.message}")
    end

    def setup_ui
      menu = UI.menu('Extensions').add_submenu('Viyar AI Agent (ВіЯр Про)')
      menu.add_item('🟢 Запустити AI-міст (TCP 127.0.0.1:9876)') { start_server }
      menu.add_item('🔴 Зупинити AI-міст') { stop_server }
      menu.add_separator
      menu.add_item('📂 Завантажити проект меблів від ШІ-агента (.json)...') { load_project_from_dialog }
      menu.add_item('📤 Експортувати поточну сцену у ViyarPro (XML + CSV)') { export_scene_from_menu }
      menu.add_separator
      menu.add_item('🧹 Очистити деталі ВіЯр у сцені') { clear_viyar_entities }
    end
  end

  unless file_loaded?(__FILE__)
    setup_ui
    start_server
    file_loaded(__FILE__)
  end
end
