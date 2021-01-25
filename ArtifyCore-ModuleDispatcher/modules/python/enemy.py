
# 1)Your program will only accept one argument - path to the images.

# 2)Then you can do any manipulations on the image

# 3)Return result


#import your lib
import cv2
import json
import sys
#import our tools
import tools

#The name of the function should be as follows
def main():
    # Your image args[1],args[2],....
    args = tools.get()

    result = some_func(args)

    # Return your result, which will return in json
    tools.return_result(result)

def some_func(args):
    # Your image args[1],args[2],....
    print(args[0])
    img = cv2.imread(args[0])

    # algorithms
    img = cv2.rotate(img, cv2.cv2.ROTATE_90_CLOCKWISE)

    # Store the resultant image
    result = cv2.imwrite(args[0], img)

    return result
if __name__ == '__main__':
    main()
