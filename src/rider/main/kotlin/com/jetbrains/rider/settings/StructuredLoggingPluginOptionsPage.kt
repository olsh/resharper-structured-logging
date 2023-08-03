package com.jetbrains.rider.settings

import com.jetbrains.rider.settings.simple.SimpleOptionsPage
import com.jetbrains.rider.settings.StructuredLoggingBundle

class StructuredLoggingPluginOptionsPage : SimpleOptionsPage(
    name = StructuredLoggingBundle.message("configurable.name.structuredlogging.title"),
    pageId = "StructuredLogging")
{
    override fun getId(): String {
        return "StructuredLogging"
    }
}
