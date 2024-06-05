using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt_badawczy {
    // MMSI,      BaseDateTime,        LAT,      LON,       SOG, COG,   Heading, VesselName, 
    // 303520000, 2021-01-01T00:00:06, 27.92347, -82.44769, 2.2, 353.3, 168.0,   SULPHUR ENTERPRISE,

    // IMO,        CallSign, VesselType, Status, Length, Width, Draft, Cargo, TranscieverClass
    // IMO9077044, KAKB,     80,         0,      159,    27,    10.1,  ,      A



    // główna klasa do rozkodowania wiadomości
    class Encoder {

        public string currentDirName = "D:/Dokumenty/Studia/03 II stopnia/I semestr/- Projekt badawczy/Dane AiS/RS232";
        // ^ lokalizacja plików z surowymi danymi AiS
        string path = @"C:\Users\bened\Desktop\testy";
        // ^ ścieżka zapisu rozkodowanych wiadomości

        // słowniki potrzebne do rozkodowywania wiadomości
        private readonly Dictionary<char, string> znaki = new();
        private readonly Dictionary<string, char> kody = new();
        private readonly Dictionary<string, char> ASCII = new();
        public Dictionary<string, AIS_Message> ships_data = new();


        // zmienna do zliczania liczby wystąpień danego typu wiadomości
        int[] ile = new int[27];

        // ustawiany zasięg poniżej którego dane są ignorowane
        readonly float RANGE = 18.0f;
        public bool ask_directory = false;


        // konstruktor klasy
        // uzupełniam słowniki odpowiednimi parami, które
        // posłużą do rozkodowywania wiadomości
        public Encoder() {

            // słownik znaki służy do zamiany znaku ASCII na kod
            // składający się z 6 bitów
            znaki.Add('0', "000000");
            znaki.Add('1', "000001");
            znaki.Add('2', "000010");
            znaki.Add('3', "000011");
            znaki.Add('4', "000100");
            znaki.Add('5', "000101");
            znaki.Add('6', "000110");
            znaki.Add('7', "000111");
            znaki.Add('8', "001000");
            znaki.Add('9', "001001");
            znaki.Add(':', "001010");
            znaki.Add(';', "001011");
            znaki.Add('<', "001100");
            znaki.Add('=', "001101");
            znaki.Add('>', "001110");
            znaki.Add('?', "001111");
            znaki.Add('@', "010000");
            znaki.Add('A', "010001");
            znaki.Add('B', "010010");
            znaki.Add('C', "010011");
            znaki.Add('D', "010100");
            znaki.Add('E', "010101");
            znaki.Add('F', "010110");
            znaki.Add('G', "010111");
            znaki.Add('H', "011000");
            znaki.Add('I', "011001");
            znaki.Add('J', "011010");
            znaki.Add('K', "011011");
            znaki.Add('L', "011100");
            znaki.Add('M', "011101");
            znaki.Add('N', "011110");
            znaki.Add('O', "011111");
            znaki.Add('P', "100000");
            znaki.Add('Q', "100001");
            znaki.Add('R', "100010");
            znaki.Add('S', "100011");
            znaki.Add('T', "100100");
            znaki.Add('U', "100101");
            znaki.Add('V', "100110");
            znaki.Add('W', "100111");
            znaki.Add('`', "101000");
            znaki.Add('a', "101001");
            znaki.Add('b', "101010");
            znaki.Add('c', "101011");
            znaki.Add('d', "101100");
            znaki.Add('e', "101101");
            znaki.Add('f', "101110");
            znaki.Add('g', "101111");
            znaki.Add('h', "110000");
            znaki.Add('i', "110001");
            znaki.Add('j', "110010");
            znaki.Add('k', "110011");
            znaki.Add('l', "110100");
            znaki.Add('m', "110101");
            znaki.Add('n', "110110");
            znaki.Add('o', "110111");
            znaki.Add('p', "111000");
            znaki.Add('q', "111001");
            znaki.Add('r', "111010");
            znaki.Add('s', "111011");
            znaki.Add('t', "111100");
            znaki.Add('u', "111101");
            znaki.Add('v', "111110");
            znaki.Add('w', "111111");

            string keys = "0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW`abcdefghijklmnopqrstuvw";

            // słownik kody zamienia 6 bitowy kod binarny na znak ASCII
            // ten kod jest związany z AIS i nie ma związku z kodem ASCII
            for (int i = 0; i < keys.Length; i++)
                kody.Add(znaki[keys[i]], keys[i]);

            // słownik ASCII zamienia 6 bitowy kod binarny na znak ASCII
            // korzysta z kodów ASCII bez dwóch MSB
            ASCII.Add("000000", ' ');
            ASCII.Add("110000", '0');
            ASCII.Add("110001", '1');
            ASCII.Add("110010", '2');
            ASCII.Add("110011", '3');
            ASCII.Add("110100", '4');
            ASCII.Add("110101", '5');
            ASCII.Add("110110", '6');
            ASCII.Add("110111", '7');
            ASCII.Add("111000", '8');
            ASCII.Add("111001", '9');
            ASCII.Add("000001", 'A');
            ASCII.Add("000010", 'B');
            ASCII.Add("000011", 'C');
            ASCII.Add("000100", 'D');
            ASCII.Add("000101", 'E');
            ASCII.Add("000110", 'F');
            ASCII.Add("000111", 'G');
            ASCII.Add("001000", 'H');
            ASCII.Add("001001", 'I');
            ASCII.Add("001010", 'J');
            ASCII.Add("001011", 'K');
            ASCII.Add("001100", 'L');
            ASCII.Add("001101", 'M');
            ASCII.Add("001110", 'N');
            ASCII.Add("001111", 'O');
            ASCII.Add("010000", 'P');
            ASCII.Add("010001", 'Q');
            ASCII.Add("010010", 'R');
            ASCII.Add("010011", 'S');
            ASCII.Add("010100", 'T');
            ASCII.Add("010101", 'U');
            ASCII.Add("010110", 'V');
            ASCII.Add("010111", 'W');
            ASCII.Add("011000", 'X');
            ASCII.Add("011001", 'Y');
            ASCII.Add("011010", 'Z');
            ASCII.Add("100000", ' ');
            ASCII.Add("101101", '-');
            ASCII.Add("101111", '/');
            ASCII.Add("101110", '.');

            // zeruję tablicę wystąpień poszczególnych typów wiadomości
            for (int j = 0; j < 27; j++)
                ile[j] = 0;
        }


        private static float rad2deg(float rad) {
            float deg = rad * 180 / (float)Math.PI;
            return deg;
        }


        private static float deg2rad(float deg) {
            float rad = deg * (float)Math.PI / 180;
            return rad;
        }


        public static float Spheral_distance(
            float LAT_1, float LON_1,
            float LAT_2 = 54.37168f, float LON_2 = 18.61250f,
            string unit = "miles") {
            float long_difference = LON_1 - LON_2;
            float distance = 0.0f;

            if (LAT_1 != LAT_2 || LON_1 != LON_2) {
                distance = 60 * 1.1515f *
                    rad2deg(
                        (float)Math.Acos(
                            (Math.Sin(deg2rad(LAT_1)) * Math.Sin(deg2rad(LAT_2))) +
                            (Math.Cos(deg2rad(LAT_1)) * Math.Cos(deg2rad(LAT_2)) *
                            Math.Cos(deg2rad(long_difference)))
                                        )
                        );
            }

            if (unit == "miles")
                return (float)Math.Round(distance, 4);

            if (unit == "kilometers")
                return (float)Math.Round(distance * 1.609344f, 3);

            return distance;
        }


        // funkcja traktuje napis składający się z 0 i 1 jak kod binarny
        // i przelicza go na wartość w systemie dziesiętnym
        private int string_to_decimal(string napis) {
            int suma = 0;
            int iloczyn = 1;    // kolejne potęgi liczby 2

            // dla każdego bitu od najmniej znaczącego
            for (int i = napis.Length - 1; i >= 0; i--) {
                // jeżeli bit ma wartość 1, to dodajemy odpowiednią
                // potęgę liczby 2 do naszej sumy
                if (napis[i] == '1')
                    suma += iloczyn;

                iloczyn *= 2;   // zwiększam potęgę liczby 2
            }

            return suma;
        }


        // funkcja traktuje napis składający się z 0 i 1 jak kod uzupełnień do dwóch
        // i przelicza go na wartość w systemie dziesiętnym
        private int u2string_to_decimal(string napis) {
            int suma = 0;
            int iloczyn = 1;    // kolejne potęgi liczby 2

            // pętla oblicza wartość dziesiętną z kodu uzupełnień do dwóch
            for (int i = napis.Length - 1; i >= 0; i--) {
                if (napis[i] == '1' && (i != 0))
                    suma += iloczyn;

                else if (napis[i] == '1' && (i == 0))
                    suma -= iloczyn;

                iloczyn *= 2;
            }

            return suma;
        }


        // funkcja wypisuje odebraną wiadomość AIS
        private void write_message(AIS_Message message) {
            Console.WriteLine("MMSI         -> " + message.MMSI);
            Console.WriteLine("BaseDateTime -> " + message.BaseDateTime);
            Console.WriteLine("LAT          -> " + message.LAT);
            Console.WriteLine("LON          -> " + message.LON);
            Console.WriteLine("SOG          -> " + message.SOG);
            Console.WriteLine("COG          -> " + message.COG);
            Console.WriteLine("Heading      -> " + message.Heading);
            Console.WriteLine("VesselName   -> " + message.VesselName);
            Console.WriteLine("IMO          -> " + message.IMO);
            Console.WriteLine("CALLSigh     -> " + message.CallSigh);
            Console.WriteLine("VesselType   -> " + message.VesselType);
            Console.WriteLine("Status       -> " + message.Status);
            Console.WriteLine("Length       -> " + message.Length);
            Console.WriteLine("Width        -> " + message.Width);
            Console.WriteLine("Draft        -> " + message.Draft);
            Console.WriteLine("Cargo        -> " + message.Cargo);
            Console.WriteLine("AISClass     -> " + message.AISClass);
            Console.WriteLine();
        }


        // funkcja do przeliczania prędkości nad dnem
        private float sog_converter(string napis) {
            // zamiana kodu binarnego na wartość dziesiętną
            int to_int = string_to_decimal(napis);

            // konwersja na typ float oraz dzielenie otrzymanej wartości przez 10
            float sog = Convert.ToSingle(to_int) / 10;

            return sog;
        }


        // funkcja do przeliczania kursu nad dnem
        private float cog_converter(string napis) {
            // zamiana kodu binarnego na wartość dziesiętną
            int to_int = string_to_decimal(napis);

            // konwersja na typ float oraz dzielenie otrzymanej wartości przez 10
            float cog = Convert.ToSingle(to_int) / 10;

            return cog;
        }


        // zamiana kodu binarnego na tekst według słownika ASCII
        private string ASCII_to_txt(string bits) {
            string helper;              // kolejne 6-cio bitowe fragmenty
            string result = "";         // napis wynikowy
            bool check = false;         // kontrola poprawności napisu

            // dla wszystkich 6-cio bitowych fragmentów
            for (int i = 0; i < bits.Length; i += 6) {
                // pobieram 6 kolejnych bitów
                helper = bits.Substring(i, 6);

                // wykonuję konwersję z kontrolą braku wystąpienia kodu w słowniku
                try {
                    result += this.ASCII[helper];
                }
                // jeżeli znaku nie ma w słowniku, to zastępuję go znakiem @
                // i ustawiam flagę check, co oznacza, że pojawił się nieznany znak
                catch {
                    result += '@';
                    check = true;
                }
            }

            // jeżeli w napisie pojawił się co najmniej jeden nieznany znak, 
            // to wypisuję ten napis
            if (check)
                Console.WriteLine(result);

            // wycinam niepotrzebne zera z końca napisu
            result = result.TrimEnd();

            return result;
        }


        // funkcja zlicza wystąpienie danych rodzajów wiadomości
        private void count_occurrence(char znak) {
            try {
                switch (znak) {
                    case '1': ile[0] += 1; break;
                    case '2': ile[1] += 1; break;
                    case '3': ile[2] += 1; break;
                    case '4': ile[3] += 1; break;
                    case '5': ile[4] += 1; break;
                    case '6': ile[5] += 1; break;
                    case '7': ile[6] += 1; break;
                    case '8': ile[7] += 1; break;
                    case '9': ile[8] += 1; break;
                    case ':': ile[9] += 1; break;
                    case ';': ile[10] += 1; break;
                    case '<': ile[11] += 1; break;
                    case '=': ile[12] += 1; break;
                    case '>': ile[13] += 1; break;
                    case '?': ile[14] += 1; break;
                    case '@': ile[15] += 1; break;
                    case 'A': ile[16] += 1; break;
                    case 'B': ile[17] += 1; break;
                    case 'C': ile[18] += 1; break;
                    case 'D': ile[19] += 1; break;
                    case 'E': ile[20] += 1; break;
                    case 'F': ile[21] += 1; break;
                    case 'G': ile[22] += 1; break;
                    case 'H': ile[23] += 1; break;
                    case 'I': ile[24] += 1; break;
                    case 'J': ile[25] += 1; break;
                    case 'K': ile[26] += 1; break;
                    default: break;
                }
            }
            catch {

            }
        }


        // wypisuje liczbę wystąpień poszczególnych rodzajów wiadomości
        public void write_occurrence() {
            for (int j = 0; j < 27; j++)
                Console.WriteLine((j + 1).ToString() + " -> " + ile[j].ToString());
        }


        // wyciąga nazwę pliku z całej ścieżki dostępu
        private string find_name(string path) {
            int indeks;
            do {
                indeks = path.IndexOf('\\');
                path = path.Substring(indeks + 1);
            }
            while (path.IndexOf('\\') != -1);

            return path;
        }


        private int Save_ships_info() {

            Dictionary<string, AIS_Message>.KeyCollection ships_mmsi = ships_data.Keys;

            foreach (string mmsi in ships_mmsi) {

                using StreamWriter ship_info = File.AppendText(Path.SHIPS + "\\" + mmsi + "\\" + "info.txt");

                ship_info.WriteLine("MMSI         -> " + ships_data[mmsi].MMSI);
                ship_info.WriteLine("BaseDateTime -> " + ships_data[mmsi].BaseDateTime);
                ship_info.WriteLine("LAT          -> " + ships_data[mmsi].LAT);
                ship_info.WriteLine("LON          -> " + ships_data[mmsi].LON);
                ship_info.WriteLine("SOG          -> " + ships_data[mmsi].SOG);
                ship_info.WriteLine("COG          -> " + ships_data[mmsi].COG);
                ship_info.WriteLine("Heading      -> " + ships_data[mmsi].Heading);
                ship_info.WriteLine("VesselName   -> " + ships_data[mmsi].VesselName);
                ship_info.WriteLine("IMO          -> " + ships_data[mmsi].IMO);
                ship_info.WriteLine("CALLSigh     -> " + ships_data[mmsi].CallSigh);
                ship_info.WriteLine("VesselType   -> " + ships_data[mmsi].VesselType);
                ship_info.WriteLine("Status       -> " + ships_data[mmsi].Status);
                ship_info.WriteLine("Length       -> " + ships_data[mmsi].Length);
                ship_info.WriteLine("Width        -> " + ships_data[mmsi].Width);
                ship_info.WriteLine("Draft        -> " + ships_data[mmsi].Draft);
                ship_info.WriteLine("Cargo        -> " + ships_data[mmsi].Cargo);
                ship_info.WriteLine("AISClass     -> " + ships_data[mmsi].AISClass);

            }

            return Msg.CORRECT;
        }



        // funkcja zwraca rozkodowaną wiadomość AIS wraz z informacją czy wiadomość jest poprawna
        // na wejście podajemy ciąg danych z surowej wiadomości AIS oraz liczbę bitów do zignorowania
        public (AIS_Message_Short, bool) encode_data(string data, char spare) {
            AIS_Message_Short ais_message = new();    // wiadomość, która będzie zwracana
            AIS_Message ship_data = new();    // wiadomość, która będzie zwracana
            string binary_data_txt = "";                    // rozkodowane dane w formie binarnej
            string helper;                                  // zmienna pomocnicza
            int message_type;                               // typ wiadomości
            int delete;
            int dimension;
            int part;
            float range_in;
            string MMSI;

            // ustalam ilość bitów do zignorowania w czasie czytania wiadomośći
            switch (spare) {
                case '0': delete = 0; break;
                case '1': delete = 1; break;
                case '2': delete = 2; break;
                case '3': delete = 3; break;
                case '4': delete = 4; break;
                case '5': delete = 5; break;
                default: delete = 0; break;
            }

            // dla każdego znaku danych, za pomocą słownka
            // zamieniam znaki ASCII na kody binarne
            foreach (char znak in data)
                binary_data_txt += znaki[znak];

            // jeżeli należy zignorować jakieś bity, to je usuwam z otrzymanego ciągu bitowego
            if (delete > 0)
                binary_data_txt = binary_data_txt.Remove(binary_data_txt.Length - delete);

            // jeżeli wiadomość zawiera jakiekolwiek dane
            if (binary_data_txt.Length > 0) {
                // wyznaczam numer oznaczający rodzaj odebranej wiadomości
                helper = binary_data_txt.Substring(0, 6);
                message_type = string_to_decimal(helper);
            }
            else
                // jeżeli wiadomość nie zawiera danych, to będzie ignorowana przez kolejne instrukcje
                message_type = 0;


            // w zależności od typu wiadomości w różny sposób ją odczytujemy
            if ((message_type == 1 || message_type == 2 || message_type == 3) && binary_data_txt.Length >= 138) {
                //Console.WriteLine("1");
                // MMSI
                helper = binary_data_txt.Substring(8, 30);
                ais_message.MMSI = string_to_decimal(helper).ToString();

                // Longitude
                helper = binary_data_txt.Substring(61, 28);
                ais_message.LON = (Convert.ToSingle(u2string_to_decimal(helper)) /
                    600000.0).ToString().Replace(',', '.');

                // Latitude
                helper = binary_data_txt.Substring(89, 27);
                ais_message.LAT = (Convert.ToSingle(u2string_to_decimal(helper)) /
                    600000.0).ToString().Replace(',', '.');

                //Range
                range_in = Spheral_distance(float.Parse(ais_message.LAT.Replace('.', ',')), float.Parse(ais_message.LON.Replace('.', ',')));
                if (range_in < this.RANGE) return (ais_message, false);
                ais_message.Range = range_in.ToString().Replace(',', '.');

            }

            // dodatkowo sprawdzam, czy wiadomość ma odpowiednią długość
            else if (message_type == 5 && binary_data_txt.Length == 424) {
                //Console.WriteLine("2");
                // MMSI
                helper = binary_data_txt.Substring(8, 30);
                MMSI = string_to_decimal(helper).ToString();

                if (!ships_data.ContainsKey(MMSI))
                    ships_data.Add(MMSI, new AIS_Message());

                ship_data = ships_data[MMSI];
                ship_data.MMSI = MMSI;

                //ships_data["12345678"].LON = "123";

                //ships_data.["12345678"].LON = "sd";

                // IMO number
                helper = binary_data_txt.Substring(40, 30);
                ship_data.IMO = string_to_decimal(helper).ToString();

                // CallSign
                helper = binary_data_txt.Substring(70, 42);
                ship_data.CallSigh = ASCII_to_txt(helper);

                // VesselName
                helper = binary_data_txt.Substring(112, 120);
                ship_data.VesselName = ASCII_to_txt(helper);

                // VesselType
                helper = binary_data_txt.Substring(232, 8);
                ship_data.VesselType = string_to_decimal(helper).ToString();

                // Length
                helper = binary_data_txt.Substring(240, 9);
                dimension = string_to_decimal(helper);
                helper = binary_data_txt.Substring(249, 9);
                dimension += string_to_decimal(helper);
                ship_data.Length = dimension.ToString();

                // Width
                helper = binary_data_txt.Substring(258, 6);
                dimension = string_to_decimal(helper);
                helper = binary_data_txt.Substring(264, 6);
                dimension += string_to_decimal(helper);
                ship_data.Width = dimension.ToString();

                // Draught
                helper = binary_data_txt.Substring(294, 8);
                ship_data.Draft = (Convert.ToSingle(string_to_decimal(helper)) / 10.0).ToString().Replace(',', '.');

                // AIS Class
                ship_data.AISClass = "A";



                ships_data[MMSI] = ship_data;

                return (ais_message, false);
            }

            else if (message_type == 18 && binary_data_txt.Length >= 134) {
                //Console.WriteLine("3");
                // MMSI
                helper = binary_data_txt.Substring(8, 30);
                ais_message.MMSI = string_to_decimal(helper).ToString();

                // Longitude
                helper = binary_data_txt.Substring(57, 28);
                ais_message.LON = (Convert.ToSingle(u2string_to_decimal(helper)) /
                    600000.0).ToString().Replace(',', '.');

                // Latitude
                helper = binary_data_txt.Substring(85, 27);
                ais_message.LAT = (Convert.ToSingle(u2string_to_decimal(helper)) /
                    600000.0).ToString().Replace(',', '.');

                //Range
                range_in = Spheral_distance(float.Parse(ais_message.LAT.Replace('.', ',')), float.Parse(ais_message.LON.Replace('.', ',')));
                if (range_in < this.RANGE) return (ais_message, false);
                ais_message.Range = range_in.ToString().Replace(',', '.');

            }

            else if (message_type == 19 && binary_data_txt.Length >= 302) {
                Console.WriteLine("4");
                // MMSI
                helper = binary_data_txt.Substring(8, 30);
                ais_message.MMSI = string_to_decimal(helper).ToString();

                // Longitude
                helper = binary_data_txt.Substring(57, 28);
                ais_message.LON = (Convert.ToSingle(u2string_to_decimal(helper)) /
                    600000.0).ToString().Replace(',', '.');

                // Latitude
                helper = binary_data_txt.Substring(85, 27);
                ais_message.LAT = (Convert.ToSingle(u2string_to_decimal(helper)) /
                    600000.0).ToString().Replace(',', '.');

                if (!ships_data.ContainsKey(ais_message.MMSI))
                    ships_data.Add(ais_message.MMSI, new AIS_Message());

                ship_data = ships_data[ais_message.MMSI];
                ship_data.MMSI = ais_message.MMSI;

                // VesselName
                helper = binary_data_txt.Substring(143, 120);
                ship_data.VesselName = ASCII_to_txt(helper);

                // VesselType
                helper = binary_data_txt.Substring(263, 8);
                ship_data.VesselType = string_to_decimal(helper).ToString();

                // Length
                helper = binary_data_txt.Substring(271, 9);
                dimension = string_to_decimal(helper);
                helper = binary_data_txt.Substring(280, 9);
                dimension += string_to_decimal(helper);
                ship_data.Length = dimension.ToString();

                // Width
                helper = binary_data_txt.Substring(289, 6);
                dimension = string_to_decimal(helper);
                helper = binary_data_txt.Substring(295, 6);
                dimension += string_to_decimal(helper);
                ship_data.Width = dimension.ToString();

                // AIS Class
                ship_data.AISClass = "B";
                ship_data.VesselName = "Benek";
                ship_data.wypisz_parametry();
                ships_data[ais_message.MMSI] = ship_data;
                ships_data[ais_message.MMSI].wypisz_parametry();

                //Range
                range_in = Spheral_distance(float.Parse(ais_message.LAT.Replace('.', ',')), float.Parse(ais_message.LON.Replace('.', ',')));
                if (range_in < this.RANGE) return (ais_message, false);
                ais_message.Range = range_in.ToString().Replace(',', '.');
            }

            else if (message_type == 24 && binary_data_txt.Length >= 41) {

                // MMSI
                helper = binary_data_txt.Substring(8, 30);
                ais_message.MMSI = string_to_decimal(helper).ToString();

                // Part Number
                helper = binary_data_txt.Substring(38, 2);
                part = string_to_decimal(helper);

                if (!ships_data.ContainsKey(ais_message.MMSI))
                    ships_data.Add(ais_message.MMSI, new AIS_Message());

                ship_data = ships_data[ais_message.MMSI];
                ship_data.MMSI = ais_message.MMSI;

                if (part == 0 && binary_data_txt.Length >= 161) {
                    Console.WriteLine("5");

                    // VesselName
                    helper = binary_data_txt.Substring(40, 120);
                    ship_data.VesselName = ASCII_to_txt(helper);

                    // AIS Class
                    ship_data.AISClass = "B";
                    ship_data.VesselName = "Angelica";

                    ships_data[ais_message.MMSI] = ship_data;

                    return (ais_message, false);
                }

                else if (part == 1 && binary_data_txt.Length >= 163) {
                    // Console.WriteLine("6");
                    //if (!ships_data.ContainsKey(ais_message.MMSI))
                    //    ships_data.Add(ais_message.MMSI, ship_data);

                    //ship_data = ships_data[ais_message.MMSI];
                    //ship_data.MMSI = ais_message.MMSI;

                    // VesselType
                    helper = binary_data_txt.Substring(40, 8);
                    ship_data.VesselType = string_to_decimal(helper).ToString();

                    // CallSign
                    helper = binary_data_txt.Substring(90, 42);
                    ship_data.CallSigh = ASCII_to_txt(helper);

                    // Length
                    helper = binary_data_txt.Substring(132, 9);
                    dimension = string_to_decimal(helper);
                    helper = binary_data_txt.Substring(141, 9);
                    dimension += string_to_decimal(helper);
                    ship_data.Length = dimension.ToString();

                    // Width
                    helper = binary_data_txt.Substring(150, 6);
                    dimension = string_to_decimal(helper);
                    helper = binary_data_txt.Substring(156, 6);
                    dimension += string_to_decimal(helper);
                    ship_data.Width = dimension.ToString();

                    // AIS Class
                    ship_data.AISClass = "B";


                    ships_data[ais_message.MMSI] = ship_data;

                    return (ais_message, false);
                }

                return (ais_message, false);

            }
            // dla innych rodzajów wiadomości
            else
                // zwracam wiadomość z flagą oznaczającą niepoprawną wiadomość
                return (ais_message, false);

            if (ais_message.LAT == "91" || ais_message.LAT == null)
                return (ais_message, false);

            // jeżeli wszystko wykonało się poprawnie zwracam rozkodowaną wiadomość
            return (ais_message, true);
        }


        // funkcja rozkodowująca wszystkie pliki tekstowe znajdujące się w zadanej lokalizacji
        public void encode_all_files(string directory = null) {

            // jeżeli nie podamy ścieżki dostępu w wywołaniu funkcji, to program
            // pobiera pliki z defaultowej lokalizacji
            if (directory is null)
                directory = currentDirName;


            // pobieram nazwy wszystkich plików z zadanej lokalizacji spełniających kryteria nazwy
            string[] files; // = System.IO.Directory.GetFiles(directory, "*_01_26_RS232_0.txt");


            bool czy = false;
            // ^ zmienna sterująca


            if (ask_directory) {

                Console.WriteLine("Podaj ścieżkę dostępu do folderu z plikami .txt:");

                while (!czy) {
                    directory = Console.ReadLine();
                    czy = Directory.Exists(directory);

                    if (!czy)
                        Console.WriteLine("\nNiepoprawna ścieżka dostępu, spróbuj ponownie:");

                }

                czy = false;
                Console.WriteLine("Podaj ścieżkę do zapisu rozkodowanych danych:");

                while (!czy) {
                    this.path = Console.ReadLine();
                    czy = Directory.Exists(this.path);

                    if (!czy)
                        Console.WriteLine("\nNiepoprawna ścieżka dostępu, spróbuj ponownie:");

                }
            }
            else {
                if (!Directory.Exists(directory)) {
                    Console.WriteLine("Niepoprawna ścieżka wejściowa");
                    return;
                }

                if (!Directory.Exists(this.path)) {
                    Console.WriteLine("Niepoprawna ścieżka wyjściowa");
                    return;
                }
            }

            files = Directory.GetFiles(directory, "*.txt");
            // ^ ścieżki i nazwy wszystkich plików tekstowych z lokalizacji directory 
            string name;
            // ^ zmienna pomocnicza
            string day_data_str;
            float day_100_avg;
            float day_10_avg;

            // dla każdego pliku tekstowego
            foreach (string filename in files) {
                // ^ dla każdego pliku tekstowego 

                name = find_name(filename);
                // ^ ze ścieżki dostępu filename pobiera samą nazwę pliku
                // instancja klasy do zapisywania danych do pliku oraz pierwsza linia nowego pliku
                // zapisuję w formacie csv, żeby można było łatwo wczytać ponownie do interpretacji
                using StreamWriter writer = File.CreateText(Path.DAYS + "\\" + name);

                writer.WriteLine("MMSI,BaseDateTime,LAT,LON,Range");

                // wczytuję cały plik
                string text = File.ReadAllText(filename);

                // i rozdzielam na poszczególne linie
                string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                // zmienne pomocnicze
                string line;
                string data;
                char message_index = ' ';
                char message_ID = ' ';
                string long_message = "";
                bool correct;
                float ship_top10_avg = 0f;

                // nowa wiadomość AIS
                AIS_Message message = new();
                AIS_Message_Short short_message = new();


                // dla każdej linii, czyli dla każdej ramki wiadomości AIS
                for (int i = 0; i < lines.Length; i++) {
                    correct = false;
                    // ^ określa, czy rozkodowana wiadomość jest poprawna
                    line = lines[i];
                    // ^ jedna linia tekstu, czyli jedna ramka

                    // pobieramy resztę wiadomości i rozdzielamy poszczególne sekcje
                    data = line[27..];
                    string[] splited = data.Split(',', '*');

                    // jeżeli wiadomość składa się tylko z jednej linijki
                    if (splited[0][0] == '1' && splited.Length == 7) {

                        /* rozkodowujemy wiadomość
                         * splited[4] - dane
                         * splited[5][0] - liczba bitów dopełnienia
                        */

                        (short_message, correct) = encode_data(splited[4], splited[5][0]);

                        // zapisujemy datę i czas odebrania wiadomości
                        short_message.BaseDateTime = line.Substring(0, 10) + "T" + line.Substring(11, 8);

                        /**
                        // zliczamy wystąpienia poszczególnych typów wiadomości
                        try
                        {
                            count_occurrence(splited[4][0]);
                        }
                        catch
                        {

                        }
                        */

                    }
                    // jeżeli wiadomość składa się z dwóch linijek
                    else if (splited[0][0] == '2' && splited.Length == 7) {
                        // jeżeli jest to pierwsza cześć wiadomości
                        if (splited[1][0] == '1') {
                            // zapisuję pierwszą część wiadomości i jej indeks oraz ID
                            long_message = splited[4];
                            message_index = splited[1][0];
                            message_ID = splited[2][0];

                            // zliczam wystąpienia poszczególnych rodzajów wiadomości
                            /**
                            try
                            {
                                // count_occurrence(splited[4][0]);
                            }
                            catch
                            {

                            }
                            */
                        }

                        // jeżeli jest to druga część wiadomości, a poprzednia miała taki sam indeks
                        if (splited[1][0] == '2' && message_ID == splited[2][0] && message_index == '1') {
                            // dodaję drugą część wiadomości do pierwszej
                            long_message += splited[4];

                            // rozkodowuję całą wiadomość
                            (short_message, correct) = encode_data(long_message, splited[5][0]);

                            // zapisujemy datę i czas odebrania wiadomości
                            short_message.BaseDateTime = line.Substring(0, 10) + "T" + line.Substring(11, 8);
                        }
                    }


                    if (correct) {
                        // ^ jeżeli wiadomość została wygenerowana poprawnie, to zapisujemy ją do pliku

                        Position Position = new();
                        Position.DateTime = DateTime.Parse(short_message.BaseDateTime);
                        Position.LAT = float.Parse(short_message.LAT.Replace('.', ','));
                        Position.LON = float.Parse(short_message.LON.Replace('.', ','));
                        Position.Range = float.Parse(short_message.Range.Replace('.', ','));

                        if (!ships_data.ContainsKey(short_message.MMSI))
                            ships_data.Add(short_message.MMSI, new AIS_Message());

                        this.ships_data[short_message.MMSI].Pos.Add(Position);
                    }

                }


                // w tym miejscu program przerobił dane z całego jednego pliku tekstowego,
                // czyli z jednego całego dnia

                Dictionary<string, AIS_Message>.KeyCollection ships_data_keys = ships_data.Keys;

                Position pos = new();
                float max_range = 0.0f;
                SortedSet<float> top_10_ranges = new();
                SortedSet<float> top_100_ranges_of_day = new();
                SortedSet<float> top_10_ranges_of_day = new();

                using StreamWriter day_top10_stream = File.AppendText(Path.RESULTS + "\\" + "00day_top10.txt");
                using StreamWriter day_top10_avg_stream = File.AppendText(Path.RESULTS + "\\" + "01day_top10_avg.txt");
                using StreamWriter day_top100_stream = File.AppendText(Path.RESULTS + "\\" + "02day_top100.txt");
                using StreamWriter day_top100_avg_stream = File.AppendText(Path.RESULTS + "\\" + "03day_top100_avg.txt");

                // dla każdego statku danego dnia
                foreach (string key in ships_data_keys) {

                    ships_data[key].filter_data();

                    Directory.CreateDirectory(Path.SHIPS + "\\" + key);
                    using StreamWriter ship = File.AppendText(Path.SHIPS + "\\" + key + "\\" + ships_data[key].Date + ".txt");
                    using StreamWriter ship_top10_stream = File.AppendText(Path.SHIPS + "\\" + key + "_" + "top10.txt");
                    using StreamWriter ship_top1_stream = File.AppendText(Path.SHIPS + "\\" + key + "_" + "top1.txt");
                    using StreamWriter ship_top10_avg_stream = File.AppendText(Path.SHIPS + "\\" + key + "_" + "top10_avg.txt");

                    for (int i = 0; i < ships_data[key].Pos.Count; i++) {
                        pos = ships_data[key].Pos[i];

                        if (top_10_ranges.Count < 10) {
                            top_10_ranges.Add(pos.Range);
                        }
                        else if (top_10_ranges.Min < pos.Range) {

                            top_10_ranges.Remove(top_10_ranges.Min);
                            top_10_ranges.Add(pos.Range);
                        }

                        if (top_100_ranges_of_day.Count < 100) {
                            top_100_ranges_of_day.Add(pos.Range);
                        }
                        else if (top_100_ranges_of_day.Min < pos.Range) {
                            top_100_ranges_of_day.Remove(top_100_ranges_of_day.Min);
                            top_100_ranges_of_day.Add(pos.Range);
                        }

                        writer.WriteLine("{0},{1},{2},{3},{4}", key, pos.strDateTime, pos.strLAT, pos.strLON, pos.strRange);
                        ship.WriteLine("{0},{1},{2},{3},{4}", key, pos.strDateTime, pos.strLAT, pos.strLON, pos.strRange);

                    }

                    ship_top10_avg = 0f;

                    // zapisywanie danych dla statków
                    if (top_10_ranges.Count != 0) {

                        ship_top10_stream.Write(name[..10]);
                        ship_top1_stream.Write(name[..10] + ",");
                        ship_top1_stream.Write(top_10_ranges.Max.ToString().Replace(",", "."));
                        ship_top1_stream.Write("\n");

                        foreach (float el in top_10_ranges) {
                            ship_top10_avg += el;
                            ship_top10_stream.Write("," + el.ToString().Replace(",", "."));
                        }

                        ship_top10_stream.Write("\n");

                        ship_top10_avg_stream.Write("{0},{1}", name[..10], (ship_top10_avg / top_10_ranges.Count).ToString().Replace(",", "."));
                        ship_top10_avg_stream.Write("\n");
                    }

                    

                    top_10_ranges.Clear();


                    {
                        /*
                         * tutaj trzeba zapisać:
                         * -> pozycje ze wszystkich statków do jednego pliku
                         * -> pozycje z danego statku do jednego pliku
                         * -> wyliczać średni zasięg z danego dnia dla wszystkich statków
                         * -> zapisywać pozycję z największym zasięgiem
                         * -> zapisywać x pozycji z największym zasięgiem
                         * -> zapisywać największy zasięg danego statku w danym dniu
                         */

                        /*
                         * Propozycja organizacji powstających folderów:
                         *      
                         *      RESULTS ->
                         *              00extracted_day_top_100.txt
                         *              01extracted_day_top_100_average.txt
                         *              02extracted_day_top_10.txt
                         *              03extracted_day_top_10_average.txt
                         *              [MMSI]_extracted.txt
                         *              DAYS ->
                         *                              [Date]_data.txt
                         *                              .
                         *                              .
                         *                              [Date]_data.txt
                         *              SHIPS ->
                         *                             [MMSI] ->
                         *                                      [00info].txt
                         *                                      [Date]_data.txt
                         *                                      .
                         *                                      .
                         *                                      [Date]_data.txt
                         *                             [MMSI] ->

                         */
                    }
                }

                day_100_avg = 0f;
                day_top100_stream.Write(name[..10]);
                foreach (float el in top_100_ranges_of_day) {
                    day_100_avg += el;
                    day_top100_stream.Write(",{0}", el.ToString().Replace(",", "."));
                }
                day_top100_stream.Write("\n");

                day_top100_avg_stream.Write(name[..10]);
                day_top100_avg_stream.Write(",{0}\n", (day_100_avg / top_100_ranges_of_day.Count).ToString().Replace(",", "."));

                day_10_avg = 0f;
                for (int i = 0; i < 10; i++) {
                    top_10_ranges_of_day.Add(top_100_ranges_of_day.Max);
                    day_10_avg += top_100_ranges_of_day.Max;
                    top_100_ranges_of_day.Remove(top_100_ranges_of_day.Max);
                }

                day_top10_stream.Write(name[..10]);
                foreach (float el in top_10_ranges_of_day) {
                    day_top10_stream.Write(",{0}", el.ToString().Replace(",", "."));
                }
                day_top10_avg_stream.Write(name[..10]);
                day_top10_avg_stream.Write(",{0}", (day_10_avg / 10).ToString().Replace(",", "."));
                Console.WriteLine("{0} -> {1}", name[..10], (day_10_avg / 10).ToString().Replace(",", "."));

                day_top10_stream.Write("\n");
                day_top10_avg_stream.Write("\n");

                top_10_ranges_of_day.Clear();
                top_100_ranges_of_day.Clear();

                foreach (string key in ships_data_keys) {

                    ships_data[key].Pos.Clear();

                }

                Save_ships_info();
                // ^ Zapisuję informacje o statkach.
                // wypisuję liczbę wystąpień poszczególnych typów wiadomości
                /// write_occurrence();
            }

            // MMSI,      BaseDateTime,        LAT,      LON,       SOG, COG,   Heading, VesselName, 
            // 303520000, 2021-01-01T00:00:06, 27.92347, -82.44769, 2.2, 353.3, 168.0,   SULPHUR ENTERPRISE,

            // IMO,        CallSign, VesselType, Status, Length, Width, Draft, Cargo, TranscieverClass
            // IMO9077044, KAKB,     80,         0,      159,    27,    10.1,  ,      A

        }

    }
}
