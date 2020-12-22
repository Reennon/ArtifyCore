
# 1)Your program will only accept one argument - path to the images.

# 2)Then you can do any manipulations on the image

# 3)Return result


#import your lib
import cv2
import json
import sys

#The name of the function should be as follows
def my_algorithm(arr):

    # Your image arr[1],arr[2],....
    img = cv2.imread(arr[1])

    # algorithms
    img = cv2.rotate(img, cv2.cv2.ROTATE_90_CLOCKWISE)

    # Store the resultant image
    result = cv2.imwrite(arr[1], img)

    # Return your result, which will return in json
    return result