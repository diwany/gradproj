using System.Collections.Generic;
using UnityEngine;

namespace Museum.Lobby
{
    [CreateAssetMenu(fileName = "Countries", menuName = "Museum/Country List")]
    public class CountryList : ScriptableObject
    {
        [System.Serializable]
        public struct Country
        {
            public string code;
            public string name;
            public Country(string code, string name) { this.code = code; this.name = name; }
        }

        public List<Country> countries = new List<Country>();

        public static List<Country> Default()
        {
            return new List<Country>
            {
                // North America
                new Country("US","United States"), new Country("CA","Canada"), new Country("MX","Mexico"),
                new Country("GT","Guatemala"), new Country("HN","Honduras"), new Country("SV","El Salvador"),
                new Country("NI","Nicaragua"), new Country("CR","Costa Rica"), new Country("PA","Panama"),
                new Country("CU","Cuba"), new Country("DO","Dominican Republic"), new Country("HT","Haiti"),
                new Country("JM","Jamaica"), new Country("TT","Trinidad and Tobago"), new Country("BS","Bahamas"),
                // South America
                new Country("BR","Brazil"), new Country("AR","Argentina"), new Country("CL","Chile"),
                new Country("CO","Colombia"), new Country("PE","Peru"), new Country("VE","Venezuela"),
                new Country("EC","Ecuador"), new Country("BO","Bolivia"), new Country("PY","Paraguay"),
                new Country("UY","Uruguay"),
                // Western & Northern Europe
                new Country("GB","United Kingdom"), new Country("IE","Ireland"), new Country("FR","France"),
                new Country("DE","Germany"), new Country("ES","Spain"), new Country("PT","Portugal"),
                new Country("IT","Italy"), new Country("NL","Netherlands"), new Country("BE","Belgium"),
                new Country("LU","Luxembourg"), new Country("CH","Switzerland"), new Country("AT","Austria"),
                new Country("MT","Malta"), new Country("CY","Cyprus"), new Country("AD","Andorra"),
                new Country("MC","Monaco"), new Country("LI","Liechtenstein"), new Country("SM","San Marino"),
                new Country("IS","Iceland"), new Country("SE","Sweden"), new Country("NO","Norway"),
                new Country("DK","Denmark"), new Country("FI","Finland"),
                // Central & Eastern Europe
                new Country("PL","Poland"), new Country("CZ","Czechia"), new Country("SK","Slovakia"),
                new Country("HU","Hungary"), new Country("RO","Romania"), new Country("BG","Bulgaria"),
                new Country("SI","Slovenia"), new Country("HR","Croatia"), new Country("BA","Bosnia and Herzegovina"),
                new Country("RS","Serbia"), new Country("ME","Montenegro"), new Country("MK","North Macedonia"),
                new Country("AL","Albania"), new Country("XK","Kosovo"), new Country("GR","Greece"),
                new Country("EE","Estonia"), new Country("LV","Latvia"), new Country("LT","Lithuania"),
                new Country("BY","Belarus"), new Country("UA","Ukraine"), new Country("MD","Moldova"),
                new Country("RU","Russia"), new Country("TR","Türkiye"),
                // Middle East
                new Country("IL","Israel"), new Country("PS","Palestine"), new Country("LB","Lebanon"),
                new Country("SY","Syria"), new Country("JO","Jordan"), new Country("IQ","Iraq"),
                new Country("IR","Iran"), new Country("SA","Saudi Arabia"), new Country("AE","UAE"),
                new Country("QA","Qatar"), new Country("BH","Bahrain"), new Country("KW","Kuwait"),
                new Country("OM","Oman"), new Country("YE","Yemen"),
                // North Africa
                new Country("EG","Egypt"), new Country("LY","Libya"), new Country("TN","Tunisia"),
                new Country("DZ","Algeria"), new Country("MA","Morocco"), new Country("SD","Sudan"),
                // Sub-Saharan Africa
                new Country("ET","Ethiopia"), new Country("ER","Eritrea"), new Country("SO","Somalia"),
                new Country("KE","Kenya"), new Country("UG","Uganda"), new Country("TZ","Tanzania"),
                new Country("RW","Rwanda"), new Country("BI","Burundi"), new Country("DJ","Djibouti"),
                new Country("NG","Nigeria"), new Country("GH","Ghana"), new Country("CI","Côte d'Ivoire"),
                new Country("SN","Senegal"), new Country("ML","Mali"), new Country("BF","Burkina Faso"),
                new Country("NE","Niger"), new Country("CM","Cameroon"), new Country("CD","DR Congo"),
                new Country("CG","Congo"), new Country("GA","Gabon"), new Country("AO","Angola"),
                new Country("ZM","Zambia"), new Country("ZW","Zimbabwe"), new Country("MZ","Mozambique"),
                new Country("MG","Madagascar"), new Country("MW","Malawi"), new Country("BW","Botswana"),
                new Country("NA","Namibia"), new Country("ZA","South Africa"), new Country("LS","Lesotho"),
                new Country("SZ","Eswatini"),
                // Central & South Asia
                new Country("KZ","Kazakhstan"), new Country("UZ","Uzbekistan"), new Country("TM","Turkmenistan"),
                new Country("KG","Kyrgyzstan"), new Country("TJ","Tajikistan"), new Country("AF","Afghanistan"),
                new Country("PK","Pakistan"), new Country("IN","India"), new Country("NP","Nepal"),
                new Country("BT","Bhutan"), new Country("BD","Bangladesh"), new Country("LK","Sri Lanka"),
                new Country("MV","Maldives"),
                // East & Southeast Asia
                new Country("CN","China"), new Country("HK","Hong Kong"), new Country("TW","Taiwan"),
                new Country("MN","Mongolia"), new Country("KR","South Korea"), new Country("KP","North Korea"),
                new Country("JP","Japan"), new Country("VN","Vietnam"), new Country("LA","Laos"),
                new Country("KH","Cambodia"), new Country("TH","Thailand"), new Country("MM","Myanmar"),
                new Country("MY","Malaysia"), new Country("SG","Singapore"), new Country("ID","Indonesia"),
                new Country("BN","Brunei"), new Country("PH","Philippines"), new Country("TL","Timor-Leste"),
                // Oceania
                new Country("AU","Australia"), new Country("NZ","New Zealand"), new Country("PG","Papua New Guinea"),
                new Country("FJ","Fiji"), new Country("SB","Solomon Islands"), new Country("VU","Vanuatu"),
                new Country("WS","Samoa"), new Country("TO","Tonga"),
                // Catch-all
                new Country("XX","Other / Prefer not to say"),
            };
        }
    }
}
