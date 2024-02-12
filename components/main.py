import os
import shutil
import base64
from precompile import *
dirs = list(filter(lambda x: x.endswith(".py"), os.listdir('.')))
dirs.remove("main.py")
dirs.remove("precompile.py")
dirs.remove("precompileTemplate.template.py")
print(dirs)


def run(dirs):
    for d in dirs:
        print(d)
        precompile(d)
    dirs = list(map(lambda fp: "precompile\\"+fp, dirs))
    for fp in dirs:
        pyfileData = None
        with open(fp, "r") as pyfile:
            pyfileData = pyfile.readlines()
            #print(pyfileData)
            imageIndex = None
            for lineIndex, line in enumerate(pyfileData):
                if "def get_Internal_Icon_24x24(self):" in line:
                    imageIndex = lineIndex
            imageLoc = pyfileData[imageIndex+1].lstrip()
            if not imageLoc.startswith("#imageLoc#"):
                break
            imageLoc = imageLoc.replace("#imageLoc#", "", 1).replace("\n", "", 1)
            imageVar = pyfileData[imageIndex+2]
            equalsPos = imageVar.index("=")
            imageVar = imageVar[:equalsPos+1]

            with open(imageLoc,"rb") as image:
                converted_string = base64.b64encode(image.read()).decode('utf-8')
                print(converted_string)
                imageVar = imageVar + ' "'+converted_string+'"\n'
            pyfileData[imageIndex+2] = imageVar
            # print(imageVar)
            # print(imageLoc)
        if not pyfileData: break
        with open(fp, "w") as pyfile:
            pyfile.writelines(pyfileData)

    args = ["redback.v0.0.0.2.ghpy"]+dirs
    print(args)

    print("="*200)
    try:
        import clr
        clr.CompileModules(*args)
        if True:
            os.system('"C:\\Program Files\\Rhino 7\\System\\Yak.exe" build')
            with open("manifest.yml", "r") as file:
                filestring = ""
                for line in file.readlines():
                    print(line)
                    if line.startswith("name:"):
                        line = line.replace("name:","").replace("\n","")
                        line = line.lstrip()
                        filestring = filestring+line
                        filestring = filestring+"-"
                    if line.startswith("version:"):
                        line = line.replace("version:","").replace("\n","")
                        line = line.lstrip()
                        filestring = filestring+line
                        filestring = filestring+"-any-any.yak"
                print(filestring)
            shutil.move(filestring, "..\\"+filestring)
    except Exception as e:
        print("errors", e)
        pass

run(dirs)