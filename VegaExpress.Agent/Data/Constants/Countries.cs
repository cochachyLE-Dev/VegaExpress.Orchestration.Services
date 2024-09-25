namespace VegaExpress.Agent.Data.Constants
{
    internal class Countries
    {
        public static readonly Dictionary<string, string> Names = new Dictionary<string, string>()
        {
             { "AR","Argentina" },
             { "BO","Bolivia" },
             { "BR","Brasil" },
             { "CL","Chile" },
             { "CO","Colombia" },
             { "CR","Costa Rica" },
             { "CU","Cuba" },
             { "EC","Ecuador" },
             { "SV","El Salvador" },
             { "GT","Guatemala" },
             { "HN","Honduras" },
             { "MX","México" },
             { "NI","Nicaragua" },
             { "PA","Panamá" },
             { "PY","Paraguay" },
             { "PE","Perú" },
             { "DO","República Dominicana" },
             { "UY","Uruguay" },
             { "VE","Venezuela" }
        };
        public static readonly Dictionary<string, string[]> Languages = new Dictionary<string, string[]>()
        {
            { "AR", new []{ "Español", } },
            { "BO", new []{ "Español", "Quechua", "Aymara"} },
            { "BR", new []{ "Portugués", } },
            { "CL", new []{ "Español", } },
            { "CO", new []{ "Español", } },
            { "CR", new []{ "Español", } },
            { "CU", new []{ "Español", } },
            { "EC", new []{ "Español", "Quichua"} },
            { "SV", new []{ "Español", } },
            { "GT", new []{ "Español", } },
            { "HN", new []{ "Español", } },
            { "MX", new []{ "Español", } },
            { "NI", new []{ "Español", } },
            { "PA", new []{ "Español", } },
            { "PY", new []{ "Español", "Guaraní"} },
            { "PE", new []{ "Español", "Quechua"} },
            { "DO", new []{ "Español", } },
            { "UY", new []{ "Español", } },
            { "VE", new []{ "Español", } }
        };
        public static readonly Dictionary<string, string[]> LanguageCodes = new Dictionary<string, string[]>()
        {
            { "AR", new []{ "es-ar", } },
            { "BO", new []{ "es-bo", "qu-bo", "ay-bo"} },
            { "BR", new []{ "pt-br", } },
            { "CL", new []{ "es-cl", } },
            { "CO", new []{ "es-co", } },
            { "CR", new []{ "es-cr", } },
            { "CU", new []{ "es-cu", } },
            { "EC", new []{ "es-ec", "qu-ec"} },
            { "SV", new []{ "es-sv", } },
            { "GT", new []{ "es-gt", } },
            { "HN", new []{ "es-hn", } },
            { "MX", new []{ "es-mx", } },
            { "NI", new []{ "es-ni", } },
            { "PA", new []{ "es-pa", } },
            { "PY", new []{ "es-py", "gn-py"} },
            { "PE", new []{ "es-pe", "qu-pe"} },
            { "DO", new []{ "es-do", } },
            { "UY", new []{ "es-uy", } },
            { "VE", new []{ "es-ve", } }
        };
    }
}
