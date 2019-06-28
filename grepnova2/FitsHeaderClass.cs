using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grepnova2
{
    class FitsHeaderClass
    {
        public struct FitsHeadStruct
        {
            public string keyword;
            public Type type;
            public string comment;

            public FitsHeadStruct(string a, Type b, string c)
            {
                keyword = a;
                type = b;
                comment = c;
            }
        }

        public FitsHeadStruct[] FitsHead = {
            new FitsHeadStruct ("SIMPLE", typeof(bool), "(FITS Standard) File conforms to FITS Standard."),
            new FitsHeadStruct ("BITPIX", typeof(int), "(FITS Standard) Number of bits per data pixel."),
            new FitsHeadStruct ("NAXIS",typeof(int), "(FITS Standard) Number of data axes."),
            new FitsHeadStruct ("NAXIS1",typeof(int), "(FITS Standard) Length of data axis 1 (number of pixels in a row)."),
            new FitsHeadStruct ("NAXIS2",typeof(int), "(FITS Standard) Length of data axis 2 (number of rows)."),
            new FitsHeadStruct ("BSCALE",typeof(float), "(FITS Standard) For unsigned 16-bit integer data, the value should be 1.0."),
            new FitsHeadStruct ("BZERO",typeof(int), "(FITS Standard) For unsigned 16-bit integer data, the value should be 32768."),
            new FitsHeadStruct ("END", null, "(FITS Standard) Marks the end of the header."),
            new FitsHeadStruct ("CALMNESS", typeof(string), "Calmness (seeing conditions), scale 1–5 (German: Ruhe)"),
            new FitsHeadStruct ("COORFLAG", typeof(string), "Quality flag of the recorded coordinates (right ascension and declination): 'error', 'missing', 'uncertain'."),
            new FitsHeadStruct ("DATE-AVG", typeof(string), "(FITS Standard) UT date and time of the mid-point of the first exposure (format YYYY-MM-DDThh:mm:ss)"),
            new FitsHeadStruct ("DATE-OBS", typeof(string), "(FITS Standard) UT date and time of the start of the observation (format YYYY-MM-DDThh:mm:ss, or YYYY-MM-DD if time is not specified). The date may differ from DATEORIG, because the original date usually refers to the evening of the observing night."),
            new FitsHeadStruct ("DATEORIG", typeof(string), "Original recorded date of the observation (evening date)"),
            new FitsHeadStruct ("DATEORn", typeof(string), "Original recorded date of the n-th exposure (n = 1…99), if exposures were made on multiple nights. Not used, when all exposures are from one night, given by DATEORIG."),
            new FitsHeadStruct ("DEC", typeof(string), "Declination of the telescope pointing (equinox J2000, sexagesimal format d:m:s)"),
            new FitsHeadStruct ("DEC_DEG", typeof(float), "Declination of the telescope pointing in decimal degrees (equinox J2000)"),
            new FitsHeadStruct ("DEC_DEn", typeof(float), "Declination of the telescope pointing in decimal degrees, n-th exposure (n = 1…99). Used only when different fields were exposed on the same plate."),
            new FitsHeadStruct ("DECn", typeof(string), "Declination of the telescope pointing, n-th exposure (n = 1…99). Used only when different fields were exposed on the same plate."),
            new FitsHeadStruct ("DEC-ORIG", typeof(string), "Original recorded declination of the telescope pointing (plate center)"),
            new FitsHeadStruct ("DEC-ORn", typeof(string), "Original declination of the telescope pointing during the n-th exposure (n = 1…99). Not used, if only one pointing was used."),
            new FitsHeadStruct ("DETNAM", typeof(string), "Detector name: 'photographic plate'"),
            new FitsHeadStruct ("DEVELOP", typeof(string), "Plate development information (developer, time)"),
            new FitsHeadStruct ("DISPERS", typeof(float), "Dispersion [Angstrom/mm]"),
            new FitsHeadStruct ("DT-AVGn", typeof(string), "UT date and time of the mid-point of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("DT-ENDn", typeof(string), "UT date and time of the end of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("DT-OBSn", typeof(string), "UT date and time of the start of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("EMULSION", typeof(string), "Type of the photographic emulsion"),
            new FitsHeadStruct ("EXPTIME", typeof(float), "Exposure time of the first exposure, expressed in seconds"),
            new FitsHeadStruct ("EXPTIMn", typeof(float), "Exposure time of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("FILTER", typeof(string), "Filter type"),
            new FitsHeadStruct ("FOCUS", typeof(float), "Focus value (from logbook). Used when a single value is given in the logs."),
            new FitsHeadStruct ("FOCUSn", typeof(float), "Focus value of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("FOV1", typeof(float), "Field of view along axis 1"),
            new FitsHeadStruct ("FOV2", typeof(float), "Field of view along axis 2"),
            new FitsHeadStruct ("GRATING", typeof(string), "Information about the grating used"),
            new FitsHeadStruct ("HJD-AVG", typeof(float), "Heliocentric Julian date at the mid-point of the first exposure"),
            new FitsHeadStruct ("HJD-AVn", typeof(float), "Heliocentric Julian date at the mid-point of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("INSTRUME", typeof(string), "(FITS Standard) Instrument name"),
            new FitsHeadStruct ("JD", typeof(float), "Julian date at the start of exposure 1"),
            new FitsHeadStruct ("JD-AVG", typeof(float), "Julian date at the mid-point of the first exposure"),
            new FitsHeadStruct ("JD-AVGn", typeof(float), "Julian date at the mid-point of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("JDn", typeof(float), "Julian date at the start of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("METHOD", typeof(string), "Observation method (literal text). A list of possible values is given in the WFPDB."),
            new FitsHeadStruct ("NOTES", typeof(string), "Miscellaneous notes"),
            new FitsHeadStruct ("NUMEXP", typeof(int), "Number of exposures"),
            new FitsHeadStruct ("OBJECT", typeof(string), "(FITS Standard) Name of the observed object or field. If there are more than one field observed, then the value shall be 'multiple' and individual names shall be given with the OBJECTn keywords."),
            new FitsHeadStruct ("OBJECTn", typeof(string), "Object (field) name on the n-th exposure (n = 1…99). Not used, if only one object (field) was observed."),
            new FitsHeadStruct ("OBJTYPE", typeof(string), "Object type (literal text), as listed in the WFPDB"),
            new FitsHeadStruct ("OBJTYPn", typeof(string), "Object type that corresponds to OBJECTn (n = 1…99)"),
            new FitsHeadStruct ("OBSERVAT", typeof(string), "Observatory name"),
            new FitsHeadStruct ("OBSERVER", typeof(string), "(FITS Standard) Observer name"),
            new FitsHeadStruct ("OBSNOTES", typeof(string), "Observer notes (from logbook)"),
            new FitsHeadStruct ("PLATEFMT", typeof(string), "Plate format (e.g. '9x12', '20x20')"),
            new FitsHeadStruct ("PLATENUM", typeof(string), "Plate number in original observation catalogue"),
            new FitsHeadStruct ("PLATESZ1", typeof(float), "Plate size along axis 1"),
            new FitsHeadStruct ("PLATESZ2", typeof(float), "Plate size along axis 2"),
            new FitsHeadStruct ("PLATNOTE", typeof(string), "Notes about the plate (e.g. contact copy of the original plate)"),
            new FitsHeadStruct ("PQUALITY", typeof(string), "Quality of the plate"),
            new FitsHeadStruct ("PRISM", typeof(string), "Information about the objective prism used"),
            new FitsHeadStruct ("PRISMANG", typeof(string), "Angle of the objective prism (format deg:min)"),
            new FitsHeadStruct ("RA", typeof(string), "Right ascension of the telescope pointing (equinox J2000, sexagesimal format h:m:s)"),
            new FitsHeadStruct ("RA_DEG", typeof(float), "Right ascension of the telescope pointing in decimal degrees (equinox J2000)"),
            new FitsHeadStruct ("RA_DEGn", typeof(float), "Right ascension of the telescope pointing in decimal degrees, n-th exposure (n = 1…99). Used only when different fields were exposed on the same plate."),
            new FitsHeadStruct ("RAn", typeof(string), "Right ascension of the telescope pointing, n-th exposure (n = 1…99). Used only when different fields were exposed on the same plate."),
            new FitsHeadStruct ("RA-ORIG", typeof(string), "Original recorded right ascension of the telescope pointing (plate center)"),
            new FitsHeadStruct ("RA-ORn", typeof(string), "Original right ascension of the telescope pointing during the n-th exposure (n = 1…99). Not used, if only one pointing was used."),
            new FitsHeadStruct ("SERIES", typeof(string), "Series or survey in which the plate belongs, e.g. Carte du Ciel, Kapteyn Selected Areas, etc."),
            new FitsHeadStruct ("SHARPNES", typeof(string), "Sharpness, scale 1–5 (German: Schärfe)"),
            new FitsHeadStruct ("SITEELEV", typeof(float), "Elevation of the observatory site [m]. Keyword SITEELEV is more widely used than SITEALTI."),
            new FitsHeadStruct ("SITELAT", typeof(float), "Latitude of the observing site, in decimal degrees"),
            new FitsHeadStruct ("SITELONG", typeof(float), "East longitude of the observing site, in decimal degrees"),
            new FitsHeadStruct ("SITENAME", typeof(string), "Observatory site name. Useful if the observatory has more than one observing site."),
            new FitsHeadStruct ("SKYCOND", typeof(string), "Notes on sky conditions (from logbook)"),
            new FitsHeadStruct ("TELAPER", typeof(float), "Clear aperture of the telescope [m]"),
            new FitsHeadStruct ("TELESCOP", typeof(string), "(FITS Standard) Telescope name"),
            new FitsHeadStruct ("TELFOC", typeof(float), "Focal length of the telescope [m]"),
            new FitsHeadStruct ("TELSCALE", typeof(float), "Plate scale of the telescope [arcsec/mm]"),
            new FitsHeadStruct ("TEMPERAT", typeof(float), "Air temperature (from logbook)."),
            new FitsHeadStruct ("TIMEFLAG", typeof(string), "Quality flag of the recorded observation time: 'error', 'missing', 'uncertain'."),
            new FitsHeadStruct ("TME-ORIG", typeof(string), "Original recorded time of the end of the observation. See TMS-ORIG for details."),
            new FitsHeadStruct ("TME-ORn", typeof(string), "Original recorded time of the end of the n-th exposure (n = 1…99). See TMS-ORIG for details."),
            new FitsHeadStruct ("TMS-ORIG", typeof(string), "Original recorded time of the start of the observation (format TZ hh:mm:ss, where TZ is time zone). Time zone can be 'ST' (sidereal time), 'UT' (universal time), or any time zone. Multiple time notations are separated with commas (e.g. 'UT 18:13, ST 02:44')."),
            new FitsHeadStruct ("TMS-ORn", typeof(string), "Original recorded time of the start of the n-th exposure (n = 1…99). See TMS-ORIG for details."),
            new FitsHeadStruct ("TRANSPAR", typeof(string), "Transparency, scale 1–5 (German: Durchsicht, Klarheit)"),
            new FitsHeadStruct ("WFPDB-ID", typeof(string), "Plate identification in the WFPDB"),
            new FitsHeadStruct ("YEAR", typeof(float), "Decimal year of the start of the first exposure"),
            new FitsHeadStruct ("YEAR-AVG", typeof(float), "Decimal year of the mid-point of the first exposure"),
            new FitsHeadStruct ("YEARn", typeof(float), "Decimal year of the start of the n-th exposure (n = 1…99)"),
            new FitsHeadStruct ("YR-AVGn", typeof(float), "Decimal year of the mid-point of the n-th exposure (n = 1…99)")

        };

        public Type GetFitsHeaderType(string key)
        {
            foreach(FitsHeadStruct k in FitsHead)
            {
                if (k.keyword.Equals(key)) return k.type;
            }
            return null;
        }

        public string GetFitsHeaderComment(string key)
        {
            foreach (FitsHeadStruct k in FitsHead)
            {
                if (k.keyword.Equals(key)) return k.comment;
            }
            return "";
        }

    }
}
