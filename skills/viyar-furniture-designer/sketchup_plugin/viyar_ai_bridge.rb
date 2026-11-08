# encoding: UTF-8
# frozen_string_literal: false

require 'sketchup.rb'
require 'extensions.rb'

module ViyarAIBridge
  PLUGIN_ID       = 'viyar_ai_bridge'.freeze
  PLUGIN_NAME     = 'Viyar AI Agent Bridge (ВіЯр Про + ABF + Gava + OpenCutList)'.freeze
  PLUGIN_VERSION  = '1.0.0'.freeze
  PLUGIN_DIR      = File.join(File.dirname(__FILE__), PLUGIN_ID)

  unless file_loaded?(__FILE__)
    ext = SketchupExtension.new(PLUGIN_NAME, File.join(PLUGIN_DIR, 'main.rb'))
    ext.description = 'Інтеграція ШІ-агента з SketchUp для автоматизованого проектування корпусних меблів та експорту у ViyarPro (сумісно з ABF, Gava, OpenCutList, Dynamic Components).'
    ext.version     = PLUGIN_VERSION
    ext.copyright   = '2026 Viyar AI Agent Suite'
    ext.creator     = 'Antigravity AI Furniture Agent'
    Sketchup.register_extension(ext, true)
    require File.join(PLUGIN_DIR, 'main.rb')
    file_loaded(__FILE__)
  end
end
