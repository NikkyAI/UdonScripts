#!/usr/bin/env kotlin

@file:Repository("https://repo.maven.apache.org/maven2/")
@file:DependsOn("io.github.typesafegithub:github-workflows-kt:3.7.0")

@file:Repository("https://bindings.krzeminski.it")
@file:DependsOn("actions:checkout:v4")
@file:DependsOn("softprops:action-gh-release:v2")

import io.github.typesafegithub.workflows.actions.actions.Checkout
import io.github.typesafegithub.workflows.actions.softprops.ActionGhRelease
import io.github.typesafegithub.workflows.domain.Concurrency
import io.github.typesafegithub.workflows.domain.RunnerType.UbuntuLatest
import io.github.typesafegithub.workflows.domain.actions.Action
import io.github.typesafegithub.workflows.domain.actions.CustomAction
import io.github.typesafegithub.workflows.domain.actions.RegularAction
import io.github.typesafegithub.workflows.domain.triggers.Push
import io.github.typesafegithub.workflows.domain.triggers.WorkflowDispatch
import io.github.typesafegithub.workflows.dsl.expressions.expr
import io.github.typesafegithub.workflows.dsl.workflow


workflow(
    name = "Release",
    on = listOf(
        WorkflowDispatch(),
        Push()
    ),
    sourceFile = __FILE__,
//    consistencyCheckJobConfig = ConsistencyCheckJobConfig.Disabled,
) {
    val pkgNameKey = "package-name"
    val packageName = expr("matrix.$pkgNameKey")
    val pkgPrefix = "moe.nikky."
    val build = job(
        id = "build",
        runsOn = UbuntuLatest,
        strategyMatrix = mapOf(
            pkgNameKey to listOf(
                "moe.nikky.common",
                "moe.nikky.kinetic-controls",
                "moe.nikky.kinetic-controls.audiolink",
            )
        ),
        env = mapOf(
            "packagePath" to packageName
        ),
        concurrency = Concurrency(
            group = "${expr {github.ref}}-$packageName",
            cancelInProgress = false,
        )
    ) {
        val packagePath = packageName
        val packageJsonPath = "$packageName/package.json"

        uses(name = "Check out", action = Checkout())

        val versionStep = uses(
            name = "get version from package.json",
            action = JsonFileProperties(
                file_path = packageJsonPath,
                prop_path = "version"
            )
        )
        val version = expr { versionStep.outputs.value }

        val variablesStep = run(
            name = "precompute variables",
            command = $$"""
                FULL_PACKAGE=$$packageName
                SHORT_PACKAGE=${FULL_PACKAGE#$$pkgPrefix}
                echo "tag=$SHORT_PACKAGE-$$version" >> $GITHUB_OUTPUT
                echo "zipFile=$SHORT_PACKAGE-$$version".zip >> $GITHUB_OUTPUT
                echo "unityPackage=$SHORT_PACKAGE-$$version".unitypackage >> $GITHUB_OUTPUT
            """.trimIndent()
        )
        val tag = expr(variablesStep.outputs["tag"])
        val zipFile = expr(variablesStep.outputs["zipFile"])
        val unityPackage = expr(variablesStep.outputs["unityPackage"])

        val checkTag = uses(
            name = "Check Tag exists",
            action = TagExistsAction(
                tag = tag
            ),
        )

        val tagDoesNotExist = "${checkTag.outputs.exists} == 'false'"

        run(
            condition = tagDoesNotExist,
            name = "Create Package Zip",
            workingDirectory = packagePath,
            command = $$"""
                zip -r "$${expr { github.workspace }}/$$zipFile" .
            """.trimIndent()
        )

        val metaListFile = "metaList-$packageName"

        run(
            condition = tagDoesNotExist,
            name = "Track Package Meta Files",
            command = $$"""
                find "$$packagePath/" -name \*.meta >> $$metaListFile
            """.trimIndent()
        )

        uses(
            condition = tagDoesNotExist,
            name = "Create UnityPackage",
            action = CustomAction(
                actionOwner = "pCYSl5EDgo",
                actionName = "create-unitypackage",
                actionVersion = "v1.2.3",
                inputs = mapOf(
//                    "package-path" to expr("env.unityPackage"),
                    "package-path" to unityPackage,
                    "include-files" to metaListFile
                )
            )
        )

        uses(
            condition = tagDoesNotExist,
            name = "Create Tag",
            action = CustomAction(
                actionOwner = "rickstaa",
                actionName = "action-create-tag",
                actionVersion = "v1.7.2",
                inputs = mapOf(
//                    "tag" to expr("env.version"),
                    "tag" to tag,
                )
            )
        )

        uses(
            condition = tagDoesNotExist,
            name = "Make Release",
            action = ActionGhRelease(
                files = listOf(
//                    expr("env.zipFile"),
//                    expr("env.unityPackage"),
                    zipFile,
                    unityPackage,
                    packageJsonPath,
                ),
                tagName = tag,
            )
        )
    }

    job(
        id = "build-done",
        runsOn = UbuntuLatest,
        needs = listOf(build),
    ) {
        run(
            name = "Trigger Workflow",
            env = mapOf(
                "GH_TOKEN" to expr("secrets.PAT"),
                "WORKFLOW" to "build-listing.yml",
                "REPO" to "NikkyAi/vpm",
                "REF" to "main",
            ),
            command = $$"""
                gh workflow run $WORKFLOW --repo=$REPO --ref=$REF
            """.trimIndent()
        )
    }
}

// https://github.com/marketplace/actions/detect-directory-changes
class DetectDirectoryChanges(
    private val includedPaths: List<String> = listOf(),
    private val includedExtensions: List<String> = listOf(),
    private val ifThesePathsChangeReturnAllIncludedPaths: List<String> = listOf(),
) : RegularAction<DetectDirectoryChanges.Outputs>("tchupp", "actions-detect-directory-changes", "v1") {
    override fun toYamlArguments() =
        linkedMapOf(
            "included-paths" to includedPaths.joinToString { "," },
            "included-extensions" to includedExtensions.joinToString { "," },
            "if-these-paths-change-return-all-included-paths" to ifThesePathsChangeReturnAllIncludedPaths.joinToString { "," }
        )

    override fun buildOutputObject(stepId: String) = Outputs(stepId)

    class Outputs(
        stepId: String,
    ) : Action.Outputs(stepId) {
        public val changed: String = "steps.$stepId.outputs.changed"
    }
}

// https://github.com/marketplace/actions/tag-exists-action
class TagExistsAction(
    private val tag: String,
) : RegularAction<TagExistsAction.Outputs>("mukunku", "tag-exists-action", "v1.7.0") {
    override fun toYamlArguments() =
        linkedMapOf(
            "tag" to tag,
        )

    override fun buildOutputObject(stepId: String) = Outputs(stepId)

    class Outputs(
        stepId: String,
    ) : Action.Outputs(stepId) {
        public val exists: String = "steps.$stepId.outputs.exists"
    }
}

// https://github.com/zoexx/github-action-json-file-properties
class JsonFileProperties(
    private val file_path: String,
    private val prop_path: String,
) : RegularAction<JsonFileProperties.Outputs>("zoexx", "github-action-json-file-properties", "1.0.6") {
    override fun toYamlArguments() =
        linkedMapOf(
            "file_path" to file_path,
            "prop_path" to prop_path,
        )

    override fun buildOutputObject(stepId: String) = Outputs(stepId)

    class Outputs(
        stepId: String,
    ) : Action.Outputs(stepId) {
        public val value: String = "steps.$stepId.outputs.value"
    }
}