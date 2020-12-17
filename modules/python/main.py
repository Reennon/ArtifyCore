
# 1)Your program will only accept one argument - path to the image.
# Use sys.argv[1] to get it.

# 2)Then you can do any manipulations on the image

# 3)Finally you need to save the changes in the same file


# For example:
# Your json:
#parsed_string = """{
#   "numberOfImages": 2,
#   "numberOfImage": [
#     "D:/ProjArtify/ArtifyCore/Users/Yura/image1.png",
#     "D:/ProjArtify/ArtifyCore/Users/Yura/image2.png"
#   ]
# }"""
import cv2
import json
import sys

#Your errors
Error = 0
try:

    #Input data(we parse it)
 app_json = json.loads(sys.argv[1])
 
    #Number of image
 numberOfImage = app_json['numberOfImages']
 arr = []
    #Add your filePATH to array
 for i in range(numberOfImage):
    arr.append(app_json['Image'][i])

    # Your image
 img = cv2.imread(arr[0])

    # algorithms
 img = cv2.rotate(img, cv2.cv2.ROTATE_90_CLOCKWISE)

    # Store the resultant image
 status = cv2.imwrite(arr[0], img)
 
except:
    Error = 1
finally:
    try:
        dict={}
        #Hear write your result(MUST BE "EEError" not "Error")
        dict['EEError'] = Error
        dict['Result'] = status
        
    finally:
        #Return Json-Results
        app_json = json.dumps(dict)
        f = open(r'\\.\pipe\NPtes', 'r+b', 0)
        f.write(app_json.encode())
        f.seek(0)
