# 1)Accept one argument - path to the images.

# 2)Import and invoke our script

# 3)Return json via pipeline


# For example:
# Your json:
#parsed_string = """{
#   "NumberOfImages": 2,
#   "numberOfImage": [
#     "D:/ProjArtify/ArtifyCore/Users/Yura/image1.png",
#     "D:/ProjArtify/ArtifyCore/Users/Yura/image2.png"
#   ],
#   "PathToModule": "enemy"
# }"""


import cv2
import json
import sys
    

try:
    #Your errors
    Error = 0

    #Input data(we parse it)
    app_json = json.loads(sys.argv[1])
 
    #Number of image
    numberOfImage = app_json['NumberOfImages']
    arr = []

    #Add your filePATH to array
    for i in range(numberOfImage):
        arr.append(app_json['Image'][i])
    
    #import our script
    my_script = getattr(__import__(app_json["PathToModule"]), "my_algorithm")
    
    #invoke our script
    status = my_script(arr)
 
except:
    Error = 1
finally:
    try:
        #Hear write your result
        dict={}
        dict['EEError'] = Error
        dict['Result'] = status
        
    finally:
        #Return Json-Results
        app_json = json.dumps(dict)
        f = open(r'\\.\pipe\NPtes', 'r+b', 0)
        f.write(app_json.encode())
        f.seek(0)
