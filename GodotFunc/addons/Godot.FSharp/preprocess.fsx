let scriptDir = __SOURCE_DIRECTORY__
let projectDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(scriptDir, "..", ".."))

printfn "Script directory: %s" scriptDir
printfn "Project directory: %s" projectDir

let referencePath = System.IO.Path.Combine(scriptDir, "reference.fsx")
printfn "PostBuild script path: %s" referencePath

if System.IO.File.Exists referencePath then
    let lines = System.IO.File.ReadAllLines referencePath
    let expectedLine = $"#i \"\"\"nuget: {scriptDir}\"\"\""

    printfn "Expected second line: %s" expectedLine
    
    if lines.Length > 1 then
        printfn "Current second line: %s" lines.[1]
        
        if lines.[1] <> expectedLine then
            lines.[1] <- expectedLine
            System.IO.File.WriteAllLines(referencePath, lines)
            printfn "✓ Updated second line in postbuild.fsx"
        else
            printfn "✓ Second line is already correct"
    else
        printfn "⚠ Warning: postbuild.fsx has less than 2 lines"
else
    printfn "❌ ERROR: postbuild.fsx not found at: %s" referencePath
