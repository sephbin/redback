import os
from PIL import Image
dirName = "Artline200_Fine_0.4_Freehand"
for filename in os.listdir("."):
	#print(filename)
	if filename.endswith(".png"):
		im = Image.open(filename)
		size = im.size[0]
		im.close()
		size = int(round((size/600)*25.4,0))
		# print(size)
		i = 0
		isNotSaved = True
		while isNotSaved:
			testName = "%03dmm-%03d.png"%(size, i)
			if testName not in os.listdir(dirName):
				os.rename(filename, os.path.join(dirName,testName))
				isNotSaved = False
			i += 1
			if i == 100: isNotSaved = False
