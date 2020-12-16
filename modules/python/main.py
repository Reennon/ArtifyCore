
# 1)Your program will only accept one argument - path to the image.
# Use sys.argv[1] to get it.

# 2)Then you can do any manipulations on the image

# 3)Finally you need to save the changes in the same file


# For example:
import cv2
import json
import sys
Error = 0
try:
    #Input data
 parsed_string = sys.argv[1]
    #Example
#parsed_string = """{
#   "numberOfImages": 2,
#   "numberOfImage": [
#     {
#
#      "Name": "D:/ProjArtify/ArtifyCore/Users/Yura/image1.png"
#
#     },
#     {
#       "Name": "D:/ProjArtify/ArtifyCore/Users/Yura/image2.png"
#
#    }
#   ],
#   "orderCompleted": true
# }"""

    #Parse Json
 parsed_string = json.loads(parsed_string)
 numberOfImage = int(parsed_string['numberOfImages'])
 arr = []
 for i in range(int(numberOfImage)):
    arr.append(parsed_string["numberOfImage"][1]["Name"])

    # Your image
 img = cv2.imread(arr[0])

    # algorithms
 img = cv2.rotate(img, cv2.cv2.ROTATE_90_CLOCKWISE)

    # Store the resultant image
 status = cv2.imwrite(arr[0], img)

except:
    Error = 1
finally:
    dict={}

    #Return Json-Results
    dict['EEError'] = Error
        #Hear write your result
    dict['Result'] = 'Your program successfully completed'
    app_json = json.dumps(dict)


    try:
        f = open(r'\\.\pipe\NPtes', 'r+b', 0)
        f.write(app_json.encode())
        f.seek(0)
    except:
        pass
