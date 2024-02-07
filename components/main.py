import os
import shutil
import base64
dirs = list(filter(lambda x: x.endswith(".py"), os.listdir('.')))
dirs.remove("main.py")
print(dirs)

args = ["redback.v0.0.0.1.ghpy"]+dirs

with open("ghContent\\Icon-TableMaker.png","r") as image:
    converted_string = base64.b64encode(image.read())
    print(converted_string)

try:
	import clr
	clr.CompileModules(*args)
except: pass

os.system('"C:\\Program Files\\Rhino 7\\System\\Yak.exe" build')
with open("manifest.yml", "r") as file:
    filestring = ""
    for line in file.readlines():
        #print(line)
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