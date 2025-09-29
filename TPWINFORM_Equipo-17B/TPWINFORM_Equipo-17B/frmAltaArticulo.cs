using Dominio;
using Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TPWINFORM_Equipo_17B
{
    public partial class frmAltaArticulo : Form
    {
        private Articulo articuloEnEdicion;
        public frmAltaArticulo()
        {
            InitializeComponent();
        }
        public frmAltaArticulo(Articulo articulo) : this()
        {
            articuloEnEdicion = articulo;
        }
        private void btnAceptar_Click(object sender, EventArgs e)
        {
            ArticuloNegocio negocio = new ArticuloNegocio();
            try
            {
                List<string> errores = new List<string>();

                // Validaciones
                if (string.IsNullOrWhiteSpace(txtCodigo.Text))
                    errores.Add("El código no puede estar vacío.");
                else if (txtCodigo.Text.Length > 50)
                    errores.Add("El código no puede superar los 50 caracteres.");

                if (string.IsNullOrWhiteSpace(txtNombre.Text))
                    errores.Add("El nombre no puede estar vacío.");
                else if (txtNombre.Text.Length > 50)
                    errores.Add("El nombre no puede superar los 50 caracteres.");

                if (!decimal.TryParse(txtPrecio.Text, out decimal precio) || precio < 0)
                    errores.Add("Ingrese un precio válido (mayor o igual a 0).");

                if (!string.IsNullOrWhiteSpace(txtDescripcion.Text) && txtDescripcion.Text.Length > 150)
                    errores.Add("El nombre no puede estar vacío ni superar los 150 caracteres.");

                if (cboMarca.SelectedItem == null)
                    errores.Add("Seleccione una marca.");

                if (cboCategoria.SelectedItem == null)
                    errores.Add("Seleccione una categoría.");

                if (articuloEnEdicion == null) articuloEnEdicion = new Articulo();
                // Validar código duplicado solo al agregar
                if (articuloEnEdicion.Id == 0)
                {
                    List<Articulo> todos = negocio.listar();
                    bool codigoDuplicado = todos.Any(a => a.Codigo.Equals(txtCodigo.Text.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (codigoDuplicado)
                        errores.Add("El código ya existe. Ingrese otro código.");
                }

                // Muestra los errores
                if (errores.Count > 0)
                {
                    MessageBox.Show(string.Join(Environment.NewLine, errores), "Errores de validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Asigna valores si pasó las validaciones
                articuloEnEdicion.Codigo = txtCodigo.Text.Trim();
                articuloEnEdicion.Nombre = txtNombre.Text.Trim();
                articuloEnEdicion.Descripcion = txtDescripcion.Text.Trim();
                articuloEnEdicion.Marca = (Marca)cboMarca.SelectedItem;
                articuloEnEdicion.Categoria = (Categoria)cboCategoria.SelectedItem;
                articuloEnEdicion.Precio = precio;
                articuloEnEdicion.UrlImagen = txtUrlImagen.Text.Trim();

                // Guarda
                if (articuloEnEdicion.Id > 0)
                {
                    negocio.Modificar(articuloEnEdicion);
                    MessageBox.Show("Artículo modificado correctamente.");
                }
                else
                {
                    negocio.Agregar(articuloEnEdicion);
                    MessageBox.Show("Artículo agregado correctamente.");
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void frmAltaArticulo_Load(object sender, EventArgs e)
        {
            MarcaNegocio marcaNegocio = new MarcaNegocio();
            CategoriaNegocio categoriaNegocio = new CategoriaNegocio();

            try
            {
                cboMarca.DataSource = marcaNegocio.listar();
                cboMarca.DisplayMember = "Descripcion";
                cboMarca.ValueMember = "Id";

                cboCategoria.DataSource = categoriaNegocio.listar();
                cboCategoria.DisplayMember = "Descripcion";
                cboCategoria.ValueMember = "Id";

                if (articuloEnEdicion != null)
                {
                    txtCodigo.Text = articuloEnEdicion.Codigo;
                    txtNombre.Text = articuloEnEdicion.Nombre;
                    txtDescripcion.Text = articuloEnEdicion.Descripcion;
                    txtPrecio.Text = articuloEnEdicion.Precio.ToString();
                    txtUrlImagen.Text = articuloEnEdicion.UrlImagen;

                    if (articuloEnEdicion.Marca != null)
                        cboMarca.SelectedValue = articuloEnEdicion.Marca.Id;
                    if (articuloEnEdicion.Categoria != null)
                        cboCategoria.SelectedValue = articuloEnEdicion.Categoria.Id;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar combos: " + ex.Message);
            }
        }
        private void txtUrlImagen_Leave(object sender, EventArgs e)
        {
            cargarImagen(txtUrlImagen.Text);
        }
        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {

        }
        private void pbxImagen_Leave(object sender, EventArgs e)
        {
        }
        private void cargarImagen(string url)
        {
            try
            {
                pbxImagen.Load(url);
            }
            catch
            {
                pbxImagen.Load("https://media.istockphoto.com/id/1128826884/vector/no-image-vector-symbol-missing-available-icon-no-gallery-for-this-moment.jpg?s=612x612&w=0&k=20&c=390e76zN_TJ7HZHJpnI7jNl7UBpO3UP7hpR2meE1Qd4=");
            }
        }
    }
}
