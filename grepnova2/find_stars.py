import sys
import numpy as np
import sep
import astropy.io.fits
from matplotlib import rcParams

def main():
    #get params
    if len(sys.argv) >=2:
        imgsource = sys.argv[1]
    else:
        print("No input. Exiting...")
        return
    #set  the size of the plot
    rcParams['figure.figsize'] = [10., 8.]

    # read image into standard 2-d numpy array
    data = astropy.io.fits.getdata(imgsource)
    #print(data)
    #<sep.Background> does not work with int16 or int32, so we have to convert it to float32
    data = np.array(data, dtype=np.float32 )

    # measure a spatially varying background on the image
    bkg = sep.Background(data)
    
    # subtract the background
    data_sub = data - bkg

    #object detection
    objects = sep.extract(data_sub, 1.5, err=bkg.globalrms)

    # plot an ellipse for each object
    total_list = []
    for i in range(len(objects)):
        lst = [i, objects['x'][i], objects['y'][i], objects['a'][i], objects['b'][i], objects['theta'][i]]
        total_list.append(lst)
    print("{}{}".format("data",total_list))
    
if __name__=='__main__':
    main()

