using Negocio;
using Dominio;
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
    public partial class frmPrincipal : Form
    {
        private List<Articulo> listaArticulos;
        private List<Articulo> listaArticulosOriginal;
        private Articulo articuloSeleccionado;
        private int indiceImagenActual = 0;

        public frmPrincipal()
        {
            InitializeComponent();
        } 
        private void frmPrincipal_Load(object sender, EventArgs e)
        {
            inicializarFiltros();
            cargar();
        }
        private void cargar()
        {
            ArticuloNegocio negocio = new ArticuloNegocio();
            try
            {
                listaArticulosOriginal = negocio.listar();
                listaArticulos = new List<Articulo>(listaArticulosOriginal);
                refrescarGrid(listaArticulos);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar artículos: " + ex.Message);
            }
        }
        private void refrescarGrid(List<Articulo> fuente)
        {
            dgvArticulos.DataSource = null;
            dgvArticulos.DataSource = fuente;

            dgvArticulos.Columns["UrlImagen"].Visible = false;

            if (dgvArticulos.Columns.Contains("Marca"))
                dgvArticulos.Columns["Marca"].Visible = false;

            if (dgvArticulos.Columns.Contains("Categoria"))
                dgvArticulos.Columns["Categoria"].Visible = false;

            if (dgvArticulos.Columns.Contains("MarcaDescripcion"))
                dgvArticulos.Columns["MarcaDescripcion"].HeaderText = "Marca";

            if (dgvArticulos.Columns.Contains("CategoriaDescripcion"))
                dgvArticulos.Columns["CategoriaDescripcion"].HeaderText = "Categoria";

            if (fuente != null && fuente.Count > 0)
            {
                articuloSeleccionado = fuente[0];
                indiceImagenActual = 0;
                mostrarImagen();
            }
        }
        private void inicializarFiltros()
        {
            cboCampo.Items.Clear();
            cboCampo.Items.AddRange(new object[] { "Codigo", "Nombre", "Descripcion", "Marca", "Categoria", "Precio" });
            if (cboCampo.Items.Count > 0)
                cboCampo.SelectedIndex = 0;
        }
        private void dgvArticulos_SelectionChanged_1(object sender, EventArgs e)
        {
            if (dgvArticulos.CurrentRow != null)
            {
                articuloSeleccionado = (Articulo)dgvArticulos.CurrentRow.DataBoundItem;
                indiceImagenActual = 0;
                mostrarImagen();
            }
        }
        private void cboCampo_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Muestra solo cuando el campo seleccionado es Precio
            bool esPrecio = cboCampo.SelectedItem != null && cboCampo.SelectedItem.ToString() == "Precio";
            cboPrecioCriterio.Visible = esPrecio;
            if (esPrecio)
            {
                cboPrecioCriterio.Items.Clear();
                cboPrecioCriterio.Items.AddRange(new object[] { "Mayor que", "Menor que", "Igual que", "Tiene dato", "No tiene dato" });
                cboPrecioCriterio.SelectedIndex = 0;
            }
            txtFiltro.Enabled = !(esPrecio && (cboPrecioCriterio.SelectedItem != null && (cboPrecioCriterio.SelectedItem.ToString() == "Tiene dato" || cboPrecioCriterio.SelectedItem.ToString() == "No tiene dato")));
        }
        private void cboPrecioCriterio_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool deshabilitar = cboPrecioCriterio.SelectedItem != null && (cboPrecioCriterio.SelectedItem.ToString() == "Tiene dato" || cboPrecioCriterio.SelectedItem.ToString() == "No tiene dato");
            txtFiltro.Enabled = !deshabilitar;
        }
        private void btnBuscar_Click(object sender, EventArgs e)
        {
            if (listaArticulosOriginal == null)
                return;

            string campo = cboCampo.SelectedItem != null ? cboCampo.SelectedItem.ToString() : string.Empty;
            string valor = (txtFiltro.Text ?? string.Empty).Trim();

            if (string.IsNullOrEmpty(valor))
            {
                listaArticulos = new List<Articulo>(listaArticulosOriginal);
                refrescarGrid(listaArticulos);
                return;
            }

            string v = valor.ToLowerInvariant();
            IEnumerable<Articulo> query = listaArticulosOriginal;
            switch (campo)
            {
                case "Codigo":
                    query = query.Where(a => (a.Codigo ?? string.Empty).ToLowerInvariant().Contains(v));
                    break;
                case "Nombre":
                    query = query.Where(a => (a.Nombre ?? string.Empty).ToLowerInvariant().Contains(v));
                    break;
                case "Descripcion":
                    query = query.Where(a => (a.Descripcion ?? string.Empty).ToLowerInvariant().Contains(v));
                    break;
                case "Marca":
                    query = query.Where(a => (a.MarcaDescripcion ?? string.Empty).ToLowerInvariant().Contains(v));
                    break;
                case "Categoria":
                    query = query.Where(a => (a.CategoriaDescripcion ?? string.Empty).ToLowerInvariant().Contains(v));
                    break;
                case "Precio":
                    string criterio = cboPrecioCriterio.SelectedItem != null ? cboPrecioCriterio.SelectedItem.ToString() : string.Empty;
                    if (criterio == "Tiene dato")
                        query = query.Where(a => a.Precio > 0);
                    else if (criterio == "No tiene dato")
                        query = query.Where(a => a.Precio == 0);
                    else
                    {
                        decimal numero;
                        if (!decimal.TryParse(valor, out numero))
                        {
                            MessageBox.Show("Ingrese un numero valido para el precio.");
                            return;
                        }
                        if (criterio == "Mayor que")
                            query = query.Where(a => a.Precio > numero);
                        else if (criterio == "Menor que")
                            query = query.Where(a => a.Precio < numero);
                        else // Igual que
                            query = query.Where(a => a.Precio == numero);
                    }
                    break;
            }

            listaArticulos = query.ToList();
            refrescarGrid(listaArticulos);
        }
        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            txtFiltro.Text = string.Empty;
            if (cboCampo.Items.Count > 0)
                cboCampo.SelectedIndex = 0;
            cargar();
        }
        private void mostrarImagen()
        {
            if (articuloSeleccionado.Imagenes.Count > 0)
                cargarImagen(articuloSeleccionado.Imagenes[indiceImagenActual].Imagen_URL);
            else
                cargarImagen("");

            actualizarBotones();
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
        private void actualizarBotones()
        {
            btnAnterior.Enabled = indiceImagenActual > 0;
            btnSiguiente.Enabled = indiceImagenActual < articuloSeleccionado.Imagenes.Count - 1;
        }
        private void btnAnterior_Click(object sender, EventArgs e)
        {
            if (indiceImagenActual > 0)
            {
                indiceImagenActual--;
                mostrarImagen();
            }
        }
        private void btnSiguiente_Click(object sender, EventArgs e)
        {
            if (indiceImagenActual < articuloSeleccionado.Imagenes.Count - 1)
            {
                indiceImagenActual++;
                mostrarImagen();
            }
        }
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            frmAltaArticulo alta = new frmAltaArticulo();
            alta.ShowDialog();
            cargar();
        }
        private void btnEliminarFisico_Click(object sender, EventArgs e)
        {
            ArticuloNegocio negocio = new ArticuloNegocio();
            try
            {
                Articulo seleccionado = (Articulo)dgvArticulos.CurrentRow.DataBoundItem; ;
                if (seleccionado == null) return;
                   
                DialogResult respuesta = MessageBox.Show(
                    "¿Estas seguro que queres eliminar el Articulo?", 
                    "Eliminando", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Warning
                );
                
                if(respuesta == DialogResult.Yes)
                {
                    negocio.Eliminar(seleccionado.Id);
                    cargar();
                }
            }
            catch (Exception ex)
            {   
                MessageBox.Show("Error al eliminar un articulo: " + ex.ToString());
            }
        }
        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvArticulos.CurrentRow == null)
                return;

            Articulo seleccionado = (Articulo)dgvArticulos.CurrentRow.DataBoundItem;
            frmAltaArticulo editar = new frmAltaArticulo(seleccionado);
            editar.ShowDialog();
            cargar();
        }
        private void dgvArticulos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvArticulos.CurrentRow != null)
            {
                Articulo articuloSeleccionado = (Articulo)dgvArticulos.CurrentRow.DataBoundItem;

                // Recargar imágenes completas
                ArticuloNegocio negocio = new ArticuloNegocio();
                articuloSeleccionado = negocio.listar()
                    .FirstOrDefault(a => a.Id == articuloSeleccionado.Id);

                frmDetalleProducto detalle = new frmDetalleProducto(articuloSeleccionado);
                detalle.ShowDialog();
            }
        }
    }
}
