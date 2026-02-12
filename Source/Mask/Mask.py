from PIL import Image
from pathlib import Path
import os

def FileExists(path: str) -> bool:
    return Path(path).is_file()

def GetBody(name: str) -> str | None:
    parts = name.split("_")
    return None if parts[2] == "Female" else parts[1]

def GetFacing(name: str) -> str | None:
    parts = name.split("_")
    parts = parts[-1].split(".")
    return parts[-2]

import processing
from processing import SIZE

dirInputBodies = r"..\..\Textures\Things\Pawn\Humanlike\Bodies"
dirInputBreasts = r"..\..\Textures\Things\Pawn\Humanlike\Breasts"
dirOutput = r"..\..\Textures\Masks\Pawn\Humanlike\Bodies"

def MakeBodyMasks():
    backgroundImage = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 255))

    for bodyName in os.listdir(dirInputBodies):
        facing = GetFacing(bodyName)
        body = GetBody(bodyName)

        if body == None or facing == None:
            continue

        bodyPath = os.path.join(dirInputBodies, bodyName)
        OutputMaskPath = os.path.join(dirOutput, bodyName.replace("Naked", "Mask"))

        bodyImage = processing.Load(bodyPath)
        bodyMask = processing.Mask(bodyImage, 255)

        bodyOutline = processing.Outline(bodyMask)
        bodyOutline = processing.Mix(bodyMask, bodyOutline)
        bodyOutline = Image.alpha_composite(backgroundImage, bodyOutline)
        bodyOutline.save(OutputMaskPath)

        breastNames = [body + "_0", body + "_1", body + "_2", body + "_3", body + "_4"]
        for breastName in breastNames:
            breastInput = os.path.join(dirInputBreasts, "Breasts_" + breastName + "_" + facing + ".png")
            if not FileExists(breastInput):
                continue

            OutputMaskPath = os.path.join(dirOutput, "Mask_" + breastName + "_" + facing + ".png")

            breastMask = processing.Load(breastInput)
            breastMask = processing.Mask(breastMask, 0, 255)




            # breastOutline = processing.Outline(breastMask)
            # breastOutline = processing.Mix(bodyMask, breastOutline)
            # breastOutline = Image.alpha_composite(backgroundImage, breastOutline)
            # breastOutline.save(OutputMaskPath)



            # breastMask = Image.open(breastInput).convert("RGBA")
            # breastMask = processing.Mask(breastMask, 0, 255)

            if facing == "north":
                breastMask = Image.alpha_composite(breastMask, bodyMask)
            else:
                breastMask = processing.Mix(bodyMask, breastMask)
            
            
            breastOutline = processing.Outline(breastMask)
            breastOutline = processing.Mix(breastMask, breastOutline)
            breastOutline = Image.alpha_composite(backgroundImage, breastOutline)

            breastOutline.save(OutputMaskPath)
            
            # breastBurn = BurnProcess(breastMask, isNorth)

            # if not isNorth:
            #     breastBurn= Image.alpha_composite(breastBurn, ThickenOutline(breastBurn, 1))
            #     breastOutline = Image.open(breastInput).convert("RGBA")
            #     breastOutline = OutlineProcess(breastOutline)
            #     breastOutline = ThickenOutline(breastOutline, 1)

            #     breastBurn = Image.alpha_composite(bodyBurn, breastOutline)

            #     if isSouth:
            #         breastOverlayInput = os.path.join(dirOutput, "Burn_" + body + "_Overlay" + ".png")
            #         if not FileExists(breastOverlayInput):
            #             continue
            #         breastOverlay = Image.open(breastOverlayInput).convert("RGBA")
            #         breastBurn = Image.alpha_composite(breastBurn, breastOverlay)

            #     breastBurn = breastBurn.filter(ImageFilter.GaussianBlur(radius=1))
            #     breastBurn = ReOutlineProcess(breastBurn)

            # breastBurn = breastBurn.filter(ImageFilter.GaussianBlur(radius=0.5))
            # breastBurn.save(bodyOutputBurn)
            
MakeBodyMasks()
