package com.jetbrains.rider.settings

import com.jetbrains.rider.settings.simple.SimpleOptionsPage

class StructuredLoggingPluginOptionsPage : SimpleOptionsPage("Structured Logging", "StructuredLogging") {
    override fun getId(): String {
        return "StructuredLogging"
    }
}
