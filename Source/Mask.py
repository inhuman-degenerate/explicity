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

def MaskProcess(image):
    width, height = image.size
    pixels = image.load()

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]

            r = 255 if a > 0 else 0
            g = 0
            b = 0
            a = 255
                
            pixels[x, y] = r, g, b, a

    return image

def BurnProcess(image, blur = True):
    arr = numpy.array(image)
    mask = arr[:, :, 0] > 0

    eroded = binary_erosion(mask, iterations=6)

    border = mask & (~eroded)
    out = numpy.zeros_like(arr)

    out[border] = [255, 0, 0, 255]
    out[~border] = [0, 0, 0, 255]

    image = Image.fromarray(out, "RGBA")

    if blur: image = image.filter(ImageFilter.GaussianBlur(radius=1))

    return image

def OutlineProcess(image):
    width, height = image.size
    pixels = image.load()

    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]

            lightness = (r + g + b) / (3 * 255)

            alpha_norm = a / 255
            alpha_weight = alpha_norm ** 0.5

            darkness = int((1 - lightness) * a)

            if a > 128:
                if darkness > 240:
                    a = 255
                    r = 255
                    g = 0
                    b = 0
                else:
                    a = 255
                    r = 0
                    g = 0
                    b = 0
            else:
                a = 0
                r = 0
                g = 0
                b = 0
            
                
            pixels[x, y] = r, g, b, a

    return image

def ThickenOutline(image, thickness):
    arr = numpy.array(image)

    outline_mask = arr[:, :, 0] > 0
    thick_mask = binary_dilation(outline_mask, iterations=thickness)

    out = numpy.zeros_like(arr)
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
        bodyMask.save(bodyOutputMask)
        bodyBurn = bodyMask
        bodyBurn = BurnProcess(bodyBurn)
        bodyBurn.save(bodyOutputBurn)

        bodyMask = Image.open(bodyInput).convert("RGBA")

        breastNames = []
        breastNames.append(body + "_0_" + facing)
        breastNames.append(body + "_1_" + facing)
        breastNames.append(body + "_2_" + facing)
        breastNames.append(body + "_3_" + facing)
        breastNames.append(body + "_4_" + facing)

        for breastName in breastNames:
            breastInput = os.path.join(dirInputBreasts, "Breasts_" + breastName + ".png")
            if not FileExists(breastInput):
                continue

            bodyOutputMask = os.path.join(dirOutput, "Mask_" + breastName + ".png")
            bodyOutputBurn = os.path.join(dirOutput, "Burn_" + breastName + ".png")

            breastMask = Image.open(breastInput).convert("RGBA")
            breastMask = Image.alpha_composite(bodyMask, breastMask)

            breastMask = MaskProcess(breastMask)
            breastMask.save(bodyOutputMask)

            isNorth = facing == "north"
            
            breastBurn = breastMask
            breastBurn = BurnProcess(breastBurn, isNorth)

            if not isNorth:
                # bodyOutline = Image.open(bodyInput).convert("RGBA")
                # bodyOutline = OutlineProcess(bodyOutline)

                breastOutline = Image.open(breastInput).convert("RGBA")
                breastOutline = OutlineProcess(breastOutline)
                breastOutline = ThickenOutline(breastOutline, 1)

                breastBurn = Image.alpha_composite(breastBurn, breastOutline)
            breastBurn = breastBurn.filter(ImageFilter.GaussianBlur(radius=0.5))
            
            breastBurn.save(bodyOutputBurn)
            
MakeBodyMasks()
