using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerraExplorerX;
using System.Xml.Linq;
using System.IO;

namespace skyline
{
    public partial class Form1 : Form
    {
        private SGWorld65 sgworld;
        public Form1()
        {
            InitializeComponent();
            sgworld = new SGWorld65();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
            
        }

        private void myScanTree()
        {
            try
            {
                var root = sgworld.ProjectTree.GetNextItem(string.Empty, ItemCode.ROOT);
                string a = sgworld.ProjectTree.GetItemName(root);
                string b = sgworld.ProjectTree.HiddenGroupName;
                BuildTreeRecursive(null, root);
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误信息：" + ex.Message);
            }
        }

        private void BuildTreeRecursive(TreeNode node, String current)
        {
            while (String.IsNullOrEmpty(current)==false)
            {
                if (sgworld.ProjectTree.IsGroup(current)) {
                    TreeNode groupnode = new TreeNode();
                    string gname = sgworld.ProjectTree.GetItemName(current);
                    groupnode.Text = gname;
                    if (node != null)
                    {
                        node.Nodes.Add(groupnode);
                    }
                    else
                    {
                        treeView1.Nodes.Add(groupnode);
                    }
                    var child = sgworld.ProjectTree.GetNextItem(current, ItemCode.CHILD);
                    BuildTreeRecursive(groupnode, child);
                }
                else
                {
                    var currentName = sgworld.ProjectTree.GetItemName(current);
                    if (node == null)
                    {
                        treeView1.Nodes.Add(currentName);
                    }
                    else
                    {
                        node.Nodes.Add(currentName);
                    }
                }
                current = sgworld.ProjectTree.GetNextItem(current, ItemCode.NEXT);
            }

        }

        private void buttonGetTree_Click(object sender, EventArgs e)
        {
            myScanTree();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TreeNode node = treeView1.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("请选择节点");
                return;
            }
            String nodeName = node.Text;
            IFeatureLayer65 layer = sgworld.ProjectTree.GetLayerByName(nodeName);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files(*.xml)|*.xml";
            saveFileDialog.DefaultExt = "xml";
            DialogResult result = saveFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                XElement element = new XElement("Layer", new object[] { new XAttribute("Name", layer.TreeItem.Name), new XAttribute("IgnoreZ", layer.IgnoreZ), new XAttribute("Reproject", layer.Reproject), new XAttribute("Streaming", layer.Streaming) });
                if (layer.FeatureGroups.Polygon != null)
                {
                    XElement content = layer.FeatureGroups.Polygon.WriteXmlSchema();
                    content.Add(new XAttribute("Name", "Polygon"));
                    element.Add(content);
                }

                if (layer.FeatureGroups.Annotation != null)
                {
                    XElement content = layer.FeatureGroups.Annotation.WriteXmlSchema();
                    content.Add(new XAttribute("Name", "Annotation"));
                    element.Add(content);
                }
                if (layer.FeatureGroups.Polyline != null)
                {
                    XElement content = layer.FeatureGroups.Polyline.WriteXmlSchema();
                    content.Add(new XAttribute("Name", "Polyline"));
                    element.Add(content);
                }
                if (layer.FeatureGroups.Point != null)
                {
                    XElement content = layer.FeatureGroups.Point.WriteXmlSchema();
                    content.Add(new XAttribute("Name", "Point"));
                    element.Add(content);
                }

                element.Save(saveFileDialog.FileName);
            }

        }

        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "FLY文件(*.fly)|*.fly";
            DialogResult result = open.ShowDialog();
            if (result == DialogResult.OK)
            {
                sgworld.Project.Open(open.FileName, true); 
            }
        }

        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            sgworld.Project.Close();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sgworld.Project.Save();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "SHP文件(*.shp)|*.shp";
            DialogResult result = open.ShowDialog();
            if (result == DialogResult.OK)
            {
                shpPath.Text = open.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "XML文件(*.xml)|*.xml";
            DialogResult result = open.ShowDialog();
            if (result == DialogResult.OK)
            {
                XMLPath.Text = open.FileName;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string spath = shpPath.Text;
            string xpath = XMLPath.Text;
            if (!File.Exists(spath))
            {
                MessageBox.Show("shp文件不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(xpath))
            {
                MessageBox.Show("XML文件不存在", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string gid = sgworld.ProjectTree.FindOrCreateGroup("demo");

            string tConnectionString = String.Format("FileName={0};TEPlugName=OGR;", spath);

            IFeatureLayer65 featurelayer = sgworld.Creator.CreateFeatureLayer(Path.GetFileNameWithoutExtension(spath), tConnectionString, gid);
            //featurelayer.Streaming = false;
            featurelayer.DataSourceInfo.Attributes.ImportAll = true;
            //featurelayer.Refresh();
            string config = xpath;
            
            if (radioYes.Checked)
            {
                featurelayer.Annotation = true;
            }
            if (radioNo.Checked)
            {
                featurelayer.Annotation = false;
            }
            
            featurelayer.ReadXmlSchema(config);
            featurelayer.Refresh();

 //           ISGWorldExtentions.sgWorld.Navigate.FlyToGuiZhou();
        }


    }
}
