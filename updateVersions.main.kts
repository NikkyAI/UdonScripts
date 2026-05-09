#!/usr/bin/env kotlin

@file:DependsOn("org.jetbrains.kotlinx:kotlinx-serialization-json-jvm:1.11.0")

import kotlinx.serialization.builtins.MapSerializer
import kotlinx.serialization.builtins.serializer
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonObject
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.jsonObject
import java.io.File

val json = Json {
    prettyPrint = true
    prettyPrintIndent = "  "
}
val versionInputFile = File("versions.json")
val versionInputs = Json.decodeFromString(
    MapSerializer(String.serializer(), String.serializer()), versionInputFile.readText()
)

println(versionInputs)

val isCI = System.getenv("CI") != null

versionInputs.keys.forEach { key ->
    val templateFile = File(key).resolve("package.template.json")

    if(!templateFile.exists()) {
        return@forEach
    }

    val packageJson = Json.parseToJsonElement(
        templateFile.readText()
    ).jsonObject.toMutableMap()

    if(isCI) {
        packageJson["version"] = JsonPrimitive(versionInputs[key])

        val vpmDeps = packageJson["vpmDependencies"]?.jsonObject?.mapValues { (k, v) ->
            if (k in versionInputs.keys) {
                JsonPrimitive(versionInputs[k])
            } else {
                v
            }
        }
        if (vpmDeps != null) {
            packageJson["vpmDependencies"] = JsonObject(vpmDeps)
        }
        templateFile.delete()
        templateFile.resolveSibling("package.template.json.meta").delete()
    }

    File(key)
        .resolve("package.json")
        .writeText(
            json.encodeToString(
                JsonObject.serializer(),

                JsonObject(
                    packageJson
                )
            )
        )
}