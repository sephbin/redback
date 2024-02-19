def precompile(path):
    import uuid
    with open("precompileTemplate.template.py", "r") as file:
        template = file.readlines()
    with open(path, "r") as file:
        script = file.readlines()

    
    #split constants from script

    constantsIndex = []
    for index, tline in enumerate(script):
        if "# Constants #" in tline:
            constantsIndex.append(index)
    print(constantsIndex)

    constants = script[constantsIndex[0]:constantsIndex[1]+1]
    constantsExec = "".join(script[constantsIndex[0]+1:constantsIndex[1]])
    #print(constants)
    #print("-"*100)
    global __var__
    __var__ = {}
    #print(constantsExec)
    exec(constantsExec) in globals(), locals()
    #print("-"*100)
    #print(__var__)
    icon = __var__["icon"]

    convertVars = []
    for inputOb in __var__["inputs"]:
        code = "{localVar} = _vars_['{localVar}']\n".format(localVar=inputOb["nickname"])
        convertVars.append(code)
    #print(convertVars)
    script=convertVars+script[constantsIndex[1]+1:]
    #print(constants)
    #print("script",script)


    scriptIndex = None
    for index, tline in enumerate(template):
        template[index] = tline.replace("print(", "#print(")
    for index, tline in enumerate(template):
        if "{runscript}" in tline:
            scriptIndex = index
            break
    #print(template[scriptIndex])
    scriptIndent = template[scriptIndex].replace("{runscript}", "").replace("\n", "")
    #print(list(scriptIndent))
    script = list(map(lambda x: scriptIndent+x, script))

    template[scriptIndex:scriptIndex] = script
    for index, tline in enumerate(template):
        if "{icon}" in tline:
            tline = tline.replace("{icon}", icon)
            template[index] = tline
        if "{guid}" in tline:
            tline = tline.replace("{guid}", str(uuid.uuid4()))
            template[index] = tline
    for index, tline in enumerate(template):
        if "{runscript}" in tline:
            scriptIndex = index
            break
    del template[scriptIndex]
    
    template = constants+template



    # outText = template.replace("{runscript}",script)
    with open("precompile\\"+path, "w") as file:
        file.writelines(template)


precompile("WriteFile.py")