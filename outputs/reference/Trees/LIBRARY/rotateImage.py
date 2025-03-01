from PIL import Image
for r in range(37):
	with Image.open(r"X:\03_Resources\Rendering\Trees\LIBRARY\WC-22-001\SHADOWBeauty.png") as im:
		with Image.open(r"X:\03_Resources\Rendering\Trees\LIBRARY\WC-22-001\SHADOWHead.png") as head:
			newImage = im.rotate(0)
			newHead = head.rotate(-10*r)
			newImage.paste(newHead, (0,-100),newHead)
			newImage = newImage.rotate(10*r)
			imgFP = "X:\\03_Resources\\Rendering\\Trees\\LIBRARY\\WC-22-001\\SHADOWBeauty0%02d0.png"%(r)
			with open(imgFP,"w") as nf:
				pass
			newImage.save(imgFP, "PNG")