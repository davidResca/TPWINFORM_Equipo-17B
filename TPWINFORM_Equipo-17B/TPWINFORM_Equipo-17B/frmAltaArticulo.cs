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
        private int indiceImagen = 0;

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
                    errores.Add("La descripción no puede superar los 150 caracteres.");

                if (cboMarca.SelectedItem == null)
                    errores.Add("Seleccione una marca.");

                if (cboCategoria.SelectedItem == null)
                    errores.Add("Seleccione una categoría.");

                if (articuloEnEdicion == null) articuloEnEdicion = new Articulo();
                
                if (articuloEnEdicion.Id == 0)
                {
                    List<Articulo> todos = negocio.listar();
                    bool codigoDuplicado = todos.Any(a => a.Codigo.Equals(txtCodigo.Text.Trim(), StringComparison.OrdinalIgnoreCase));
                    if (codigoDuplicado)
                        errores.Add("El código ya existe. Ingrese otro código.");
                }

                if (errores.Count > 0)
                {
                    MessageBox.Show(string.Join(Environment.NewLine, errores), "Errores de validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Asigna si pasa validaciones
                articuloEnEdicion.Codigo = txtCodigo.Text.Trim();
                articuloEnEdicion.Nombre = txtNombre.Text.Trim();
                articuloEnEdicion.Descripcion = txtDescripcion.Text.Trim();
                articuloEnEdicion.Marca = (Marca)cboMarca.SelectedItem;
                articuloEnEdicion.Categoria = (Categoria)cboCategoria.SelectedItem;
                articuloEnEdicion.Precio = precio;

                // Actualiza lista img
                articuloEnEdicion.Imagenes.Clear();
                foreach (ListViewItem item in listImg.Items)
                {
                    if (item.Tag is Imagen img)
                        articuloEnEdicion.Imagenes.Add(img);
                }

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

                    // Carga img en List View
                    listImg.Items.Clear();
                    foreach (Imagen img in articuloEnEdicion.Imagenes)
                    {
                        ListViewItem item = new ListViewItem(img.Imagen_URL);
                        item.Tag = img;
                        listImg.Items.Add(item);
                    }

                    // Muestra primer img en Pbx --> si existe
                    if (listImg.Items.Count > 0)
                    {
                        indiceImagen = 0;
                        listImg.Items[0].Selected = true;
                        cargarImagen(((Imagen)listImg.Items[0].Tag).Imagen_URL);
                    }
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
        private void btnAddUrlImg_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtUrlImagen.Text))
            {
                Imagen img = new Imagen()
                {
                    Imagen_URL = txtUrlImagen.Text.Trim()
                };

                // Crear un Item en la ListView
                ListViewItem item = new ListViewItem(img.Imagen_URL);
                item.Tag = img;

                listImg.Items.Add(item);

                // Si es la primera imagen, mostrarla
                if (listImg.Items.Count == 1)
                {
                    indiceImagen = 0;
                    pbxImagen.Load(img.Imagen_URL);
                }

                txtUrlImagen.Clear();
            }
        }
        private void listImg_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listImg.SelectedItems.Count > 0)
            {
                indiceImagen = listImg.SelectedItems[0].Index;
                Imagen img = (Imagen)listImg.SelectedItems[0].Tag;
                cargarImagen(img.Imagen_URL);
            }
        }
        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (listImg.Items.Count == 0) return;

            indiceImagen = (indiceImagen + 1) % listImg.Items.Count;
            listImg.Items[indiceImagen].Selected = true;
        }
        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (listImg.Items.Count == 0) return;

            indiceImagen = (indiceImagen - 1 + listImg.Items.Count) % listImg.Items.Count;
            listImg.Items[indiceImagen].Selected = true;
        }
    }
}
