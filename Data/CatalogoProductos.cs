namespace ApiPrimera.Data;

public static class CatalogoProductos
{
    public sealed record Semilla(
        string Nombre,
        string Descripcion,
        string Marca,
        string Categoria,
        int Anio,
        int Kilometraje,
        string Combustible,
        string Transmision,
        string Color,
        decimal Precio,
        decimal? PrecioOriginal,
        int Stock,
        bool Destacado,
        string ImagenOrigen);

    public const string AgenteHttp = "ApiPrimera-Ecommerce/1.0 (taller ASP.NET Core; uso educativo)";

    public static IReadOnlyList<Semilla> Semillas { get; } = new List<Semilla>
    {
        new(
            "Toyota Corolla Cross Hybrid XLE",
            "SUV compacto híbrido con tracción integral, sistema Toyota Safety y hasta 1.000 km de recorrido sin gastar gasolina. Ideal para ciudad y carretera.",
            "Toyota", "SUV", 2024, 12400, "Híbrido", "Automática", "Blanco perla",
            96400000m, 102000000m, 4, true,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/0/0b/Toyota_Corolla_Cross_Hybrid_1X7A1861.jpg/1280px-Toyota_Corolla_Cross_Hybrid_1X7A1861.jpg"),

        new(
            "Toyota RAV4 XLE Híbrido",
            "SUV mediano de cinco plazas, motor 2.5 híbrido autorrecargable y modo 4x4. Amplio maletero y pantalla táctil de 9 pulgadas.",
            "Toyota", "SUV", 2023, 24800, "Híbrido", "Automática", "Gris plata",
            118900000m, 124500000m, 3, true,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/a/af/Toyota_RAV4_XA30_Shishi_01_2022-09-13.jpg/1280px-Toyota_RAV4_XA30_Shishi_01_2022-09-13.jpg"),

        new(
            "Toyota Hilux GR Sport",
            "Camioneta pickup con motor 2.8 turbo, tracción 4x4 y paquete GR Sport con suspensión reforzada para trabajo pesado.",
            "Toyota", "Pickup", 2023, 31200, "Gasolina", "Automática", "Negro",
            132000000m, 138000000m, 2, true,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/8/81/Toyota_HiLux_GR_Sport_1X7A7281.jpg/1280px-Toyota_HiLux_GR_Sport_1X7A7281.jpg"),

        new(
            "Toyota Yaris Cross Hybrid",
            "Crossover urbano de 4.2 metros. Consume menos de 4 litros en ciudad gracias a su sistema híbrido de 48 voltios y cabe fácil en el parqueo.",
            "Toyota", "Crossover", 2024, 8100, "Híbrido", "Automática", "Azul cobalto",
            74500000m, 79000000m, 6, false,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/f/f4/Toyota_Yaris_Cross_Hybrid_%28XP210%29_1X7A1846.jpg/1280px-Toyota_Yaris_Cross_Hybrid_%28XP210%29_1X7A1846.jpg"),

        new(
            "Renault Duster Intens",
            "SUV rosero de 1.6 litros con 100 CV y gran altura libre. Es el mas vendido del segmento en Colombia.",
            "Renault", "SUV", 2023, 38600, "Gasolina", "Manual", "Gris platino",
            68900000m, 74900000m, 5, false,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/9/9e/Dacia_Duster_III_GIMS_2024_1X7A2016.jpg/1280px-Dacia_Duster_III_GIMS_2024_1X7A2016.jpg"),

        new(
            "Renault Koleos GT Line",
            "SUV de siete plazas con cabina en cuero, techo panorámico y sistema multimedia Easy Link con Apple CarPlay y Android Auto.",
            "Renault", "SUV", 2022, 52300, "Gasolina", "Automática", "Negro",
            79400000m, 85000000m, 2, false,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/5/54/Renault_Koleos_II_Facelift_1X7A1659.jpg/1280px-Renault_Koleos_II_Facelift_1X7A1659.jpg"),

        new(
            "Renault Logan Stepway",
            "Sedán familiar con altura extra de suspensión y amplia cajuela de 508 litros. Ideal para familia.",
            "Renault", "Sedán", 2023, 41500, "Gasolina", "Manual", "Plata",
            46700000m, 49500000m, 7, false,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/3/34/2020_Renault_Logan_Stepway_front.jpg/1280px-2020_Renault_Logan_Stepway_front.jpg"),

        new(
            "Renault Kwid",
            "El auto más económico de nuestra vitrina. 1.0 de tres cilindros, transmisión manual de 5 velocidades y consumo de 5.5 litros por cada 100 km.",
            "Renault", "Hatchback", 2022, 47900, "Gasolina", "Manual", "Blanco",
            34900000m, 37200000m, 9, false,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/c/cb/Renault_Kwid_%2853343921273%29.jpg/1280px-Renault_Kwid_%2853343921273%29.jpg"),

        new(
            "Chevrolet Onix Premier",
            "Sedán compacto con turbo 1.0L, caja automática de 6 velocidades y pantalla MyLink de 8 pulgadas compatible con CarPlay.",
            "Chevrolet", "Sedán", 2024, 9200, "Gasolina", "Automática", "Rojo carmín",
            62900000m, 66500000m, 4, true,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/5/55/2023_Chevrolet_Onix_Plus_1.0T_Premier_AT.jpg/1280px-2023_Chevrolet_Onix_Plus_1.0T_Premier_AT.jpg"),

        new(
            "Chevrolet Tracker LTZ",
            "SUV turbo 1.2L de 130 CV con cinco modos de conducción, MyLink y seis airbags. La opción más equilibrada de la vitrina.",
            "Chevrolet", "SUV", 2023, 28400, "Gasolina", "Automática", "Azul noche",
            71500000m, 76000000m, 3, false,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/6/66/2022_Chevrolet_Tracker_1.2_Turbo_LTZ%2C_front_%28Argentina%29.jpg/1280px-2022_Chevrolet_Tracker_1.2_Turbo_LTZ%2C_front_%28Argentina%29.jpg"),

        new(
            "Chevrolet Spark GT",
            "Hatchback urbano ideal para la ciudad. Cuenta con aire acondicionado, dirección asistida y seis parlantes con conexión bluetooth.",
            "Chevrolet", "Hatchback", 2022, 35700, "Gasolina", "Manual", "Gris",
            32400000m, 34900000m, 8, false,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/c/c4/2019_Chevrolet_Spark_1LT%2C_front_left%2C_12-10-2022.jpg/1280px-2019_Chevrolet_Spark_1LT%2C_front_left%2C_12-10-2022.jpg"),

        new(
            "Chevrolet Captiva Premier",
            "SUV de siete plazas con motor 2.0L, techo panorámico y siete airbags. Vehículo revisado, con un año de garantía.",
            "Chevrolet", "SUV", 2021, 64300, "Gasolina", "Automática", "Blanco",
            58900000m, 64000000m, 0, false,
            "https://thumb.wikimedia.org/wikipedia/commons/thumb/4/44/CHEVROLET_CAPTIVA_China_%287%29.jpg/1280px-CHEVROLET_CAPTIVA_China_%287%29.jpg")
    };
}
