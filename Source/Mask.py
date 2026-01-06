import numpy
from PIL import Image, ImageFilter
from scipy.ndimage import binary_erosion, binary_dilation
from pathlib import Path
import os

def GetBody(name: str) -> str | None:
    parts = name.split("_")

    if parts[2] == "Female": return None

    return parts[1]

def GetFacing(name: str) -> str | None:
    parts = name.split("_")
    
    if "east" in name: return "east"
    if "north" in name: return "north"
    if "south" in name: return "south"

    return None

def MaskProcess(image, overlay = False):
    width, height = image.size
    pixels = image.load()

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]

            a / 4

            r = 255 if a > 63 and not overlay else 0
            g = 255 if a > 60 and overlay else 0
            a = 255 if r == 255 or g == 255 else 0
            if overlay and g == 0:
                a = 0

                #  if g == 0 else 0
                
            pixels[x, y] = r, g, 0, a

    return image

def BurnProcess(image, blur = True):
    arr = numpy.array(image)
    mask = (arr[:, :, 0] > 0) | (arr[:, :, 1] > 0)
    eroded = binary_erosion(mask, iterations=6)

    border = mask & (~eroded)
    out = numpy.zeros_like(arr)

    out[border] = [255, 0, 0, 255]
    out[~border] = [0, 0, 0, 255]

    image = Image.fromarray(out, "RGBA")

    return image

def MixProcess(image, image2 = None) -> Image:

    background = Image.new("RGBA", (256, 256), (0, 0, 0, 255))
    image = Image.alpha_composite(background, image)
    if image2 != None: image = Image.alpha_composite(image, image2)

    return image

def clamp(n, min_val, max_val):
    return max(min_val, min(n, max_val))

def OutlineProcess(image):
    width, height = image.size
    pixels = image.load()

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]

            darkness = 255 - ((r + g + b) // 3)

            # alpha_norm = a / 255
            # alpha_weight = alpha_norm ** 0.5

            # darkness = int((1 - lightness) * a)

            if a > 32:
                if darkness > 240:
                    a = 255
                    r = 255
                else:
                    a = 255
                    r = 0
            else:
                a = 0
                r = 0
                
            pixels[x, y] = r, 0, 0, a

    return image

def ReOutlineProcess(image):
    width, height = image.size
    pixels = image.load()

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]

            r = 255 if r > 120 else 0
                
            pixels[x, y] = r, 0, 0, 255

    return image

def ThickenOutline(image, thickness):
    arr = numpy.array(image)
    outline_mask = arr[:, :, 0] > 0

    thick_mask = binary_dilation(outline_mask, iterations=thickness)

    out = arr.copy()
    out[thick_mask] = [255, 0, 0, 255]

    return Image.fromarray(out, "RGBA")

def MakeDir(path: str):
    Path(path).mkdir(parents=True, exist_ok=True)

def FileExists(path: str) -> bool:
    return Path(path).is_file()

def MakeBodyMasks():
    dirInputBodies = r"..\Textures\Things\Pawn\Humanlike\Bodies"
    dirInputBreasts = r"..\Textures\Things\Pawn\Humanlike\Breasts"
    dirOutput = r"..\Textures\Masks\Pawn\Humanlike\Bodies"

    for bodyName in os.listdir(dirInputBodies):
        body = GetBody(bodyName)
        facing = GetFacing(bodyName)

        if body == None or facing == None:
            continue

        bodyNameMask = bodyName.replace("Naked", "Mask")
        bodyNameBurn = bodyName.replace("Naked", "Burn")
        bodyInput = os.path.join(dirInputBodies, bodyName)
        bodyOutputMask = os.path.join(dirOutput, bodyNameMask)
        bodyOutputBurn = os.path.join(dirOutput, bodyNameBurn)

        bodyMask = Image.open(bodyInput).convert("RGBA")
        bodyMask = MaskProcess(bodyMask)
        bodySaveMask = MixProcess(bodyMask)
        bodySaveMask.save(bodyOutputMask)

        bodyBurn = BurnProcess(bodyMask)
        bodyBurn.save(bodyOutputBurn)

        breastNames = []
        breastNames.append(body + "_0_" + facing)
        breastNames.append(body + "_1_" + facing)
        breastNames.append(body + "_2_" + facing)
        breastNames.append(body + "_3_" + facing)
        breastNames.append(body + "_4_" + facing)

        isNorth = facing == "north"
        isSouth = facing == "south"

        for breastName in breastNames:
            breastInput = os.path.join(dirInputBreasts, "Breasts_" + breastName + ".png")
            if not FileExists(breastInput):
                continue

            bodyOutputMask = os.path.join(dirOutput, "Mask_" + breastName + ".png")
            bodyOutputBurn = os.path.join(dirOutput, "Burn_" + breastName + ".png")

            breastMask = Image.open(breastInput).convert("RGBA")
            breastMask = MaskProcess(breastMask, True)

            if isNorth:
                breastMask = MixProcess(breastMask, bodyMask)
            else:
                breastMask = MixProcess(bodyMask, breastMask)

            breastMask.save(bodyOutputMask)
            
            breastBurn = BurnProcess(breastMask, isNorth)

            if not isNorth:
                breastBurn= Image.alpha_composite(breastBurn, ThickenOutline(breastBurn, 1))
                breastOutline = Image.open(breastInput).convert("RGBA")
                breastOutline = OutlineProcess(breastOutline)
                breastOutline = ThickenOutline(breastOutline, 1)

                breastBurn = Image.alpha_composite(bodyBurn, breastOutline)

                if isSouth:
                    breastOverlayInput = os.path.join(dirOutput, "Burn_" + body + "_Overlay" + ".png")
                    if not FileExists(breastOverlayInput):
                        continue
                    breastOverlay = Image.open(breastOverlayInput).convert("RGBA")
                    breastBurn = Image.alpha_composite(breastBurn, breastOverlay)

                breastBurn = breastBurn.filter(ImageFilter.GaussianBlur(radius=1))
                breastBurn = ReOutlineProcess(breastBurn)

            breastBurn = breastBurn.filter(ImageFilter.GaussianBlur(radius=0.5))
            breastBurn.save(bodyOutputBurn)
            
MakeBodyMasks()
