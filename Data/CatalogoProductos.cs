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
            "Toyota", "SUV", 2024, 0, "Híbrido", "Automática", "Blanco perla",
            96400000m, 102000000m, 4, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591254/productos/sy7lvvhuetk8sl8qfomd.jpg"),

        new(
            "Toyota RAV4 XLE Híbrido",
            "SUV mediano de cinco plazas, motor 2.5 híbrido autorrecargable y modo 4x4. Amplio maletero y pantalla táctil de 9 pulgadas.",
            "Toyota", "SUV", 2023, 0, "Híbrido", "Automática", "Gris plata",
            118900000m, 124500000m, 3, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591255/productos/apnvhyehe4llrcqdlely.jpg"),

        new(
            "Toyota Hilux GR Sport",
            "Camioneta pickup con motor 2.8 turbo, tracción 4x4 y paquete GR Sport con suspensión reforzada para trabajo pesado.",
            "Toyota", "Pickup", 2023, 0, "Gasolina", "Automática", "Negro",
            132000000m, 138000000m, 2, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591257/productos/ji5wppelta7ihtjcrkdo.jpg"),

        new(
            "Toyota Yaris Cross Hybrid",
            "Crossover urbano de 4.2 metros. Consume menos de 4 litros en ciudad gracias a su sistema híbrido de 48 voltios y cabe fácil en el parqueo.",
            "Toyota", "Crossover", 2024, 0, "Híbrido", "Automática", "Azul cobalto",
            74500000m, 79000000m, 6, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591258/productos/lqaxukahee8juun3xo1n.jpg"),

        new(
            "Renault Duster Intens",
            "SUV rosero de 1.6 litros con 100 CV y gran altura libre. Es el mas vendido del segmento en Colombia.",
            "Renault", "SUV", 2023, 0, "Gasolina", "Manual", "Gris platino",
            68900000m, 74900000m, 5, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591259/productos/hhpkkmcnyimll3lpmieb.jpg"),

        new(
            "Renault Koleos GT Line",
            "SUV de siete plazas con cabina en cuero, techo panorámico y sistema multimedia Easy Link con Apple CarPlay y Android Auto.",
            "Renault", "SUV", 2022, 0, "Gasolina", "Automática", "Negro",
            79400000m, 85000000m, 2, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591260/productos/hgxmzu7qtzmrppdivmry.jpg"),

        new(
            "Renault Logan Stepway",
            "Sedán familiar con altura extra de suspensión y amplia cajuela de 508 litros. Ideal para familia.",
            "Renault", "Sedán", 2023, 0, "Gasolina", "Manual", "Plata",
            46700000m, 49500000m, 7, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591262/productos/isdyrjymnimablabrjr2.jpg"),

        new(
            "Renault Kwid",
            "Compacto urbano 1.0 de tres cilindros, transmisión manual de 5 velocidades y consumo de 5.5 litros por cada 100 km. Una de las opciones mas economicas del mercado colombiano.",
            "Renault", "Hatchback", 2022, 0, "Gasolina", "Manual", "Blanco",
            55990000m, 58990000m, 9, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591263/productos/nchma4ksjwoail3imid5.jpg"),

        new(
            "Chevrolet Onix Premier",
            "Sedán compacto con turbo 1.0L, caja automática de 6 velocidades y pantalla MyLink de 8 pulgadas compatible con CarPlay.",
            "Chevrolet", "Sedán", 2024, 0, "Gasolina", "Automática", "Rojo carmín",
            62900000m, 66500000m, 4, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591264/productos/meobaimcfcniryteyic9.jpg"),

        new(
            "Chevrolet Tracker LTZ",
            "SUV turbo 1.2L de 130 CV con cinco modos de conducción, MyLink y seis airbags. La opción más equilibrada de la vitrina.",
            "Chevrolet", "SUV", 2023, 0, "Gasolina", "Automática", "Azul noche",
            71500000m, 76000000m, 3, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591266/productos/bvr0ncccmrnyezwkpywc.jpg"),

        new(
            "Chevrolet Spark GT",
            "Hatchback urbano ideal para la ciudad. Cuenta con aire acondicionado, dirección asistida y seis parlantes con conexión bluetooth.",
            "Chevrolet", "Hatchback", 2022, 0, "Gasolina", "Manual", "Gris",
            32400000m, 34900000m, 8, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591267/productos/wfwjy7ub8dm6lkeas1qr.jpg"),

        new(
            "Chevrolet Captiva Premier",
            "SUV de siete plazas con motor 2.0L, techo panorámico y siete airbags. Vehículo revisado, con un año de garantía.",
            "Chevrolet", "SUV", 2021, 0, "Gasolina", "Automática", "Blanco",
            58900000m, 64000000m, 0, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591268/productos/lghi7og2lxqfv9umoxlf.jpg"),

        new(
            "Tesla Model Y Long Range",
            "SUV 100% eléctrico con 533 km de autonomía, techo panorámico de vidrio y Autopilot. Carga del 10 al 80 por ciento en unos 27 minutos.",
            "Tesla", "SUV", 2024, 0, "Eléctrico", "Automática", "Blanco perla",
            189900000m, 199900000m, 1, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591270/productos/dz2zdknjwbzr5exsnx1h.jpg"),

        new(
            "Kia K3 Cross GT Line",
            "Crossover compacto con motor 1.6L, caja automática de 6 velocidades y pantalla de 10.25 pulgadas. Uno de los mas vendidos de Kia en Colombia.",
            "Kia", "Crossover", 2024, 0, "Gasolina", "Automática", "Gris grafito",
            89900000m, 94900000m, 3, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591273/productos/eoi6gveh8vryxlzn5tuy.png"),

        new(
            "Kia Picanto",
            "Hatchback económico 1.0L con aire acondicionado, pantalla táctil y bajo consumo. Perfecto para moverse en la ciudad.",
            "Kia", "Hatchback", 2023, 0, "Gasolina", "Manual", "Rojo",
            55900000m, 58900000m, 6, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591275/productos/xyytmwe5ioyi4599kowp.jpg"),

        new(
            "Foton Tunland G7",
            "Camioneta pickup 4x4 de trabajo con motor 2.0 turbo diésel, doble cabina y capacidad de carga de una tonelada.",
            "Foton", "Pickup", 2023, 0, "Diésel", "Manual", "Blanco",
            84900000m, 89900000m, 2, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591277/productos/j9ndba2q1bjozwsybact.jpg"),

        new(
            "Mazda 2 Skyactiv",
            "Hatchback premium de manejo ágil, motor 1.5L Skyactiv con 115 CV y acabados en cuero. Referente de calidad en su segmento.",
            "Mazda", "Hatchback", 2024, 0, "Gasolina", "Automática", "Azul polímtero",
            64900000m, 68900000m, 4, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591278/productos/cdtlwf1bhw7ebgitnnc7.jpg"),

        new(
            "Lexus NX 450h+",
            "SUV de lujo híbrido enchufable con 306 caballos, techo panorámico y asientos ventilados. Máxima eficiencia y confort.",
            "Lexus", "SUV", 2023, 0, "Híbrido", "Automática", "Negro",
            249900000m, 259900000m, 1, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591280/productos/dvwexffoirvmbxrjl7ou.jpg"),

        new(
            "Subaru Forester e-Boxer",
            "SUV con tracción integral permanente simétrica, motor boxer y sistema híbrido e-Boxer. Uno de los mas seguros de la vitrina.",
            "Subaru", "SUV", 2024, 0, "Híbrido", "Automática", "Verde oliva",
            129900000m, 136900000m, 2, false,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591282/productos/qfswqnmpzqcxc0bt8ljw.jpg"),

        new(
            "Ford Ranger Wildtrak",
            "Pickup 4x4 diésel con 210 caballos, doble cabina y capacidad de remolque de 3.5 toneladas. La mas equipada de todas las Rangers.",
            "Ford", "Pickup", 2023, 0, "Diésel", "Automática", "Azul",
            149900000m, 158900000m, 2, true,
            "https://res.cloudinary.com/cpzb6ci1/image/upload/v1791591284/productos/hszvvyljyuthwdvhg2di.jpg")
    };
}
