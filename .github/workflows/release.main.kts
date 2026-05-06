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
import io.github.typesafegithub.workflows.domain.Shell
import io.github.typesafegithub.workflows.domain.actions.Action
import io.github.typesafegithub.workflows.domain.actions.CustomAction
import io.github.typesafegithub.workflows.domain.actions.RegularAction
import io.github.typesafegithub.workflows.domain.triggers.Push
import io.github.typesafegithub.workflows.domain.triggers.WorkflowDispatch
import io.github.typesafegithub.workflows.dsl.expressions.expr
import io.github.typesafegithub.workflows.dsl.workflow
import io.github.typesafegithub.workflows.yaml.ConsistencyCheckJobConfig


workflow(
    name = "Release",
    on = listOf(
        WorkflowDispatch(),
        Push()
    ),
    sourceFile = __FILE__,
//    consistencyCheckJobConfig = ConsistencyCheckJobConfig.Disabled,
) {
//    job(id = "config", runsOn = UbuntuLatest) {
//        uses(name = "Check out", action = Checkout())
//        run(name = "Print greeting", command = "echo 'Hello world!'")
//    }
//    val checkConsistency = job(
//        name = "Check YAML consistency",
//        id = "check_yaml_consistency",
//        runsOn = UbuntuLatest,
//        concurrency = Concurrency(
//            group = "consistency",
//            cancelInProgress = true
//        ),
//    ) {
//        uses(
//            name = "Check out",
//            action = Checkout(),
//        )
//        run(
//            name = "Execute script",
//            command = """
//                rm '.github/workflows/release.yaml' && '.github/workflows/release.main.kts'
//            """.trimIndent()
//        )
//        run(
//            name = "Consistency check",
//            command = """
//                git diff --exit-code '.github/workflows/release.yaml'
//            """.trimIndent()
//        )
//        //  check_yaml_consistency:
//        //    name: ''
//        //    runs-on: 'ubuntu-latest'
//        //    steps:
//        //    - id: 'step-0'
//        //      name: 'Check out'
//        //      uses: 'actions/checkout@v4'
//        //    - id: 'step-1'
//        //      name: 'Execute script'
//        //      run: 'rm ''.github/workflows/release.yaml'' && ''.github/workflows/release.main.kts'''
//        //    - id: 'step-2'
//        //      name: 'Consistency check'
//        //      run: 'git diff --exit-code ''.github/workflows/release.yaml'''
//    }

    val build = job(
        id = "build",
        runsOn = UbuntuLatest,
//        needs = listOf(checkConsistency),
//        outputs =
//            object : JobOutputs() {
//                var tagCommon by output()
//                var tagKineticControls by output()
//                var changedKinecticControls by output()
//                var tagKineticControlsAudiolink by output()
//            },
        strategyMatrix = mapOf(
            "package-name" to listOf(
                "moe.nikky.common",
                "moe.nikky.kinetic-controls",
                "moe.nikky.kinetic-controls-audiolink",
            )
        ),
        env = mapOf(
            "packagePath" to expr("matrix.package-name")
        ),
        concurrency = Concurrency(
            group = "${expr {github.ref}}-${expr("matrix.package-name")}",
        )
    ) {
        val packageName = expr("matrix.package-name")
        val packagePath = packageName
        val packageJsonPath = "$packageName/package.json"
        uses(name = "Check out", action = Checkout(fetchTags = true))

        val versionStep = uses(
            name = "get version from package.json",
            action = JsonFileProperties(
                file_path = packageJsonPath,
                prop_path = "version"
            )
        )
        val computeTag = run(
            name = "compute tag",
            shell = Shell.Bash,
            command = $$"""
                PACKAGE=$$packageName
                echo "tag=${PACKAGE#moe.nikky.}-$${expr { versionStep.outputs.value }}" >> $GITHUB_OUTPUT
            """.trimIndent()
        )
        val combinedTag = expr(computeTag.outputs["tag"])

        val checkTag = uses(
            name = "Check Tag exists",
            action = TagExistsAction(
                tag = combinedTag
            ),
        )

        val tagDoesNotExist = "${checkTag.outputs.exists} == 'false'"

        run(
            condition = tagDoesNotExist,
            name = "Set Environment Variables",
            command = $$"""
                echo "zipFile=$$packageName-$${expr { versionStep.outputs.value }}".zip >> $GITHUB_ENV
                echo "unityPackage=$$packageName-$${expr { versionStep.outputs.value }}.unitypackage" >> $GITHUB_ENV
                echo "version=$${expr { versionStep.outputs.value }}" >> $GITHUB_ENV
            """.trimIndent()
        )

        run(
            condition = tagDoesNotExist,
            name = "Create Package Zip",
            //TODO: use outputs of previous step ?
            command = $$"""
                zip -r "${{ github.workspace }}/${{ env.zipFile }}" .
            """.trimIndent()
        )

        run(

            condition = tagDoesNotExist,
            name = "Track Package Meta Files",
            //TODO: use outputs of previous step ?
            command = $$"""
                find "$$packagePath/" -name \*.meta >> metaList
            """.trimIndent()
        )

        uses(
            condition = tagDoesNotExist,
            name = "Create UnityPackage",
            //TODO: use outputs of previous step ?
            action = CustomAction(
                actionOwner = "pCYSl5EDgo",
                actionName = "create-unitypackage",
                actionVersion = "v1.2.3",
                inputs = mapOf(
                    "package-path" to expr("env.unityPackage"),
                    "include-files" to "metaList"
                )
            )
        )

        uses(
            condition = tagDoesNotExist,
            name = "Create Tag",
            //TODO: use outputs of previous step instead of env ?
            action = CustomAction(
                actionOwner = "rickstaa",
                actionName = "action-create-tag",
                actionVersion = "v1.7.2",
                inputs = mapOf(
//                    "tag" to expr("env.version"),
                    "tag" to combinedTag,
                )
            )
        )

        uses(
            condition = tagDoesNotExist,
            name = "Make Release",
            //TODO: use outputs of previous step instead of env?
            action = ActionGhRelease(
                files = listOf(
                    expr("env.zipFile"),
                    expr("env.unityPackage"),
                    packageJsonPath,
                ),
                tagName = combinedTag,
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