using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace Argus_CRM.Clases
{
    public class Administrador : Usuario
    {
        public String strCorreo_electronicop; //*
        public int intidAdmin; //*
        //public int intCiudad;
        public string strExtension = ""; //*
        public Administrador()
        {
        }

        public Administrador(SqlDataReader datos)
        {
            intidAdmin = (int)datos["id_admin"];
            intTipousuario = (int)datos["tipo_usuario"];
            strNombre = (String)datos["nombre"];
            strApellido = (String)datos["apellido"];
            strCorreo = (String)datos["correo_electronico"];
            strCorreo_electronicop = (String)datos["correo_electronicop"];
            strCargo = (String)datos["cargo"];
            //intRegion = (int)datos["region"];
            //strPais = (String)datos["pais"];
            intCiudad = (int)datos["ciudad"];
            //strEstado = (String)datos["estado"];
            strUsuario = (String)datos["usuario"];
            strContrasena = (String)datos["contrasena"];
            strDireccion = (String)datos["direccion"];
            strTelefonop = (String)datos["telefono1"];
            strTelefonocelular = (String)datos["telefono2"];
            strRadio = (String)datos["telefono3"];

            intStatus = (int)datos["status"];
            System.Diagnostics.Debug.WriteLine(intStatus);
        }
    }
}