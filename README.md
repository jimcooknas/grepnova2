# grepnova2
GrepNova2 is an image manipulation tool, created bt <i>Dominic Ford</i>, which aids the image comparison tasks which are required for amateur nova-supernova hunting

<b>Overview for grepnova</b>

GrepNova is an image manipulation tool which aids the image comparison tasks which are required for amateur nova/supernova hunting. It is operated by supplying pairs of images, each consisting of a reference library image of an object and a newly obtained image of the same object. GrepNova will align the two images into a common orientation and then and blink them, allowing easy visual comparison, as is necessary when visually searching images of galaxies for new sources which might be nova/supernova candidates. Because such patrolling, by its very nature, involves searching large numbers of images, GrepNova includes features to ease the processing many pairs of images in quick succession.

GrepNova accepts image files in two formats: FITS  (preferred) and JPEG. In either case, GrepNova only works with monochrome images, and color JPEGs will be greyscaled upon being loaded. The first input image, henceforth called the template, should be a library image of the target galaxy, in which it is assumed that there are no supernovae. The second image, henceforth called the subject, is the new image that is to be searched for new objects. These two images do not need to be of the same size, but they do need to be of the same magnification (i.e. each pixel width must correspond to the same angle on the sky in both images). They may also be at any arbitrary orientation with respect to each other, as GrepNova is able to correct for any relative translation (lateral shift) or rotation between the two images, using a pattern recognition algorithm applied to field stars to align them. It should be noted that there is a modest requirement on the size of the region of overlap between the images to ensure that a sufficient number of common field stars are identified for this pattern recognition to succeed. GrepNova is also able to detect and correct for any differences between the seeing and sky transparency conditions of the two images.

It is hoped that a future version of GrepNova might be able to perform fully automated searching of images for supernovae. The last version, 1.0.0, however, serves only to assist with the task of aligning pairs of images at speed and blinking them, leaving the user to visually search for nova/supernova candidates.

GrepNova is available for multiple platforms. A graphical interface is available for the Microsoft Windows and Linux operating systems. A Linux/UNIX command-line utility version is also available for grepnova.

<b>Overview for Grepnova2</b>

Grepnova2 initially started as grepnova version 1.0.0, re-coding Dominic’s code, as this code (version 0.6.0) was kindly published under GNU General Public License. Although the code was understandable and finely commented, the main obstacle was the GTK disadvantages under Windows, if someone intended to extend grepnova’s GUI for this platform (as I was). So Grepnova2 started from scratch in C# (don’t ask me why I chose this C variant), keeping in mind Dominic’s concept and GUI layout. As GSL library could not be ported in Windows development environment (like Visual Studio 2017, where I had to compile it’s source from scratch in Windows), I decided to use parts of grepnova’s routines and code (especially Dominic’s likelihood.c) as a standalone executable (compiled under Linux as Windows executable) to calculate the displacement and rotation angle for the aligned templates. I truly admit that Dominic’s alignment code under Linux is so lightning fast that no development under Windows could even come close to it. On the other hand, the only library I succeeded to find to replace CFITSIO for C# was the CSharpFITS  package, that is a pure C# .NET port of Tom McGlynn’s nom.tam.fits Java package. It provides native C# support for reading and writing FITS files.

Respecting Dominic Ford’s grepnova (as this was an inspiration source) I kept the Users’s Guide template and format according to initial Dominic’s Guide. I also kept intact Dominic’s wording in all places of this Guide that refer to original grepnova.

<b>Setup folder</b>
The folder <b>GrepNova2 (copy-paste to new location)</b> is a folder containing all executable files, compiled in MS Visual Studio 2019 (Windows 10). Copy all these files in a folder of your choice and run the <b>grepnova2.exe</b> executable
  
  <b>Sample files</b>
  In folder <b>FITS</b> there are some sample files (FITS images) in folders <b>NEW</b> and <b>REF</b>, just for starting playing around.
