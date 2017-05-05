using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Argus_CRM.Clases
{
    public class Accesorio : Material
    {

        public Accesorio(): base()
        {

        }
        public Accesorio(String nombre,String marca, String modelo, String ficha, float costo): base(nombre,marca,modelo,ficha,costo)
        {
            
        }

    }
}