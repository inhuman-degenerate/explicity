from PIL import Image, ImageChops, ImageFilter

SIZE = 128

def Load(path):
    image = Image.open(path).convert("RGBA")
    width, height = image.size
    if width != SIZE:
        image = image.resize((SIZE, SIZE), resample=Image.NEAREST)

    return image

def Mask(image, red = 255, green = 0):
    width, height = image.size
    img = image.copy()
    pixels = img.load()
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            if (a > 0):
                r = red
                g = green
                a = 255
            else:
                r = 0
                g = 0
                a = 0
            pixels[x, y] = r, g, 0, a
    return img

def Outline(image):
    width, height = image.size
    pixels = image.load()
    img = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 255))
    pixels2 = img.load()

    for y in range(height):
        for x in range(width):
            if x == 0 or y == 0: continue
            if x == width-1 or y == height-1: continue
            if pixels[x, y][0] == 255 or pixels[x, y][1] == 255: continue
            r, g, b, a = pixels[x, y]
            if IsOutline(pixels, x, y):
                b = 255

            pixels2[x, y] = 0, 0, b, 255

    img = img.filter(ImageFilter.GaussianBlur(radius=0.8))
    pixels2 = img.load()
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels2[x, y]
            if b > 0:
                b = 255
            else:
                b = 0

            pixels2[x, y] = 0, 0, b, 255

    # img = img.filter(ImageFilter.GaussianBlur(radius=0.8))
    return img
    #         r, g, b, a = pixels[x, y]
    #         darkness = 255 - ((r + g + b) // 3)
            
    #         if a > 0 and darkness > 250:
    #             r = 255
    #         else:
    #             r = 0

    #         pixels[x, y] = r, 0, 0, 255

def IsOutline(pixels, x, y) -> bool:
    if pixels[x-1, y][0] == 255 or pixels[x+1, y][0] == 255 or pixels[x, y-1][0] == 255 or pixels[x, y+1][0] == 255: return True
    if pixels[x-1, y][1] == 255 or pixels[x+1, y][1] == 255 or pixels[x, y-1][1] == 255 or pixels[x, y+1][1] == 255: return True

    return False;



def Mix(image, image2):
    return ImageChops.add(image, image2)

#     width, height = image.size
#     pixels = image.load()
#     black = (0, 0, 0, 255)
#     color = (255, 0, 0, 255)
#     outline = (0, 0, 255, 255)

#     for y in range(height):
#         for x in range(width):
#             if x == 0 or y == 0: continue
#             if x == width-1 or y == height-1: continue

#             r, g, b, a = pixels[x, y]

#             if r == 0 and HasRed(pixels, x, y):
#                 b = 255
#             if r == 255 and IsInline(pixels, x, y):
#                 b = 255

            

#             pixels[x, y] = r, g, b, a

#     return image





# def IsInline(pixels, x, y) -> bool:
#     r1, g1, b1, a1 = pixels[x-1, y]
#     r2, g2, b2, a2 = pixels[x+1, y]
#     r3, g3, b3, a3 = pixels[x, y-1]
#     r4, g4, b4, a4 = pixels[x, y+1]

#     if r1 == 0 or r2 == 0 or r3 == 0 or r4 == 0:
#         return True

#     return False

# def HasRed(pixels, x, y) -> bool:
#     if pixels[x-1, y][0] == 255 or pixels[x+1, y][0] == 255 or pixels[x, y-1][0] == 255 or pixels[x, y+1][0] == 255:
#         return True

#     return False

# def BurnProcess(image, blur = True):
    # width, height = image.size
    # pixels = image.load()

    # for y in range(height):
    #     for x in range(width):
    #         r, g, b, a = pixels[x, y]

    #         darkness = 255 - ((r + g + b) // 3) if a > 0 else 0
            
    #         pixels[x, y] = darkness, 0, 0, 255

    # arr = numpy.array(image)
    # mask = (arr[:, :, 0] > 0) | (arr[:, :, 1] > 0)
    # eroded = binary_erosion(mask, iterations=6)

    # border = mask & (~eroded)
    # out = numpy.zeros_like(arr)

    # out[border] = [255, 0, 0, 255]
    # out[~border] = [0, 0, 0, 255]

    # image = Image.fromarray(out, "RGBA")

#     return image

# def ReOutlineProcess(image):
#     width, height = image.size
#     pixels = image.load()

#     for y in range(height):
#         for x in range(width):
#             r, g, b, a = pixels[x, y]

#             r = 255 if r > 120 else 0
                
#             pixels[x, y] = r, 0, 0, 255

#     return image

# def ThickenOutline(image, thickness):
#     arr = numpy.array(image)
#     outline_mask = arr[:, :, 0] > 0

#     thick_mask = binary_dilation(outline_mask, iterations=thickness)

#     out = arr.copy()
#     out[thick_mask] = [255, 0, 0, 255]

#     return Image.fromarray(out, "RGBA")
