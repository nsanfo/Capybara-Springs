using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

class TileInfo
{
    //public bool up = false, down = false, left = false, right = false;
    private PlotInfo plotInfo;
    public EdgeDirectionInfo up, down, left, right;

    public TileInfo(PlotInfo plotInfo)
    {
        this.plotInfo = plotInfo;
        up = new EdgeDirectionInfo(false, -1, -1);
        down = new EdgeDirectionInfo(false, -1, -1);
        left = new EdgeDirectionInfo(false, -1, -1);
        right = new EdgeDirectionInfo(false, -1, -1);
    }

    public PlotInfo PlotInfo { get { return plotInfo; } }
}

class EdgeDirectionInfo {
    public bool isEdge { get; }
    public int xLoc { get; }
    public int yLoc { get; }

    public EdgeDirectionInfo(bool isEdge, int xLoc, int yLoc)
    {
        this.isEdge = isEdge;
        this.xLoc = xLoc;
        this.yLoc = yLoc;
    }
}

public class PlotOutsideHandler : MonoBehaviour
{
    public GameObject edgePrefab, middlePrefab, emptyEdgePrefab;
    public Material bambooMat, rockMat;

    [Header("Performance")]
    public bool generateDeco = true;

    private List<TileInfo> tiles = new();

    public void GenerateOutsideTiles(PlotInfo[,] plots)
    {
        List<TileInfo> generatedTiles = ScanTilesToGenerate(plots);
        foreach (TileInfo tile in generatedTiles)
        {
            GenerateTile(tile);
        }

        AddGeneratedTilesToList(generatedTiles);
    }

    private List<TileInfo> ScanTilesToGenerate(PlotInfo[,] plots)
    {
        // Generate the tile list
        List<TileInfo> tilesToGenerate = new();
        for (int i = 0; i < plots.GetLength(0); i++)
        {
            for (int j = 0; j < plots.GetLength(1); j++)
            {
                if (!plots[i, j].purchased)
                {
                    tilesToGenerate.Add(CreateTileInfo(plots, plots[i, j]));
                }
            }
        }

        // Compare with previous list, remove tiles that have not changed
        List<TileInfo> removeTiles = new();
        foreach (TileInfo generatedTile in tilesToGenerate)
        {
            foreach (TileInfo originalTile in tiles)
            {
                if (CompareTileChange(originalTile, generatedTile))
                {
                    removeTiles.Add(generatedTile);
                }
            }
        }

        // Remove tiles that don't need to be regenerated
        foreach (TileInfo tile in removeTiles)
        {
            tilesToGenerate.Remove(tile);
        }

        return tilesToGenerate;
    }

    private void AddGeneratedTilesToList(List<TileInfo> generatedTiles)
    {
        foreach (TileInfo generatedTile in generatedTiles)
        {
            bool existsInOriginal = false;
            foreach (TileInfo originalTile in tiles)
            {
                // Update the tile in the list with the generated tile
                if (generatedTile.PlotInfo.xLocation ==  originalTile.PlotInfo.xLocation
                    && generatedTile.PlotInfo.yLocation == originalTile.PlotInfo.yLocation)
                {
                    originalTile.up = generatedTile.up;
                    originalTile.right = generatedTile.right;
                    originalTile.down = generatedTile.down;
                    originalTile.left = generatedTile.left;
                }

                continue;
            }

            // Add the tile to the list if it doesn't exist
            if (!existsInOriginal)
            {
                tiles.Add(generatedTile);
            }
        }
    }

    private TileInfo CreateTileInfo(PlotInfo[,] plots, PlotInfo plotInfo)
    {
        TileInfo tileInfo = new TileInfo(plotInfo);

        // Check left
        if (plotInfo.xLocation - 1 > 0)
        {
            PlotInfo left = plots[plotInfo.yLocation, plotInfo.xLocation - 1];
            if (left.purchased) tileInfo.left = new EdgeDirectionInfo(true, plotInfo.xLocation - 1, plotInfo.yLocation);
        }

        // Check down
        if (plotInfo.yLocation + 1 < plots.GetLength(0))
        {
            PlotInfo down = plots[plotInfo.yLocation + 1, plotInfo.xLocation];
            if (down.purchased) tileInfo.down = new EdgeDirectionInfo(true, plotInfo.xLocation, plotInfo.yLocation + 1);
        }

        // Check right
        if (plotInfo.xLocation + 1 < plots.GetLength(1))
        {
            PlotInfo right = plots[plotInfo.yLocation, plotInfo.xLocation + 1];
            if (right.purchased) tileInfo.right = new EdgeDirectionInfo(true, plotInfo.xLocation + 1, plotInfo.yLocation);
        }

        // Check up
        if (plotInfo.yLocation - 1 > 0)
        {
            PlotInfo up = plots[plotInfo.yLocation - 1, plotInfo.xLocation];
            if (up.purchased) tileInfo.up = new EdgeDirectionInfo(true, plotInfo.xLocation, plotInfo.yLocation - 1);
        }

        return tileInfo;
    }

    private bool CompareTileChange(TileInfo originalTile, TileInfo generatedTile)
    {
        // Check direction
        if (originalTile.up.isEdge != generatedTile.up.isEdge) return false;
        if (originalTile.right.isEdge != generatedTile.right.isEdge) return false;
        if (originalTile.down.isEdge != generatedTile.down.isEdge) return false;
        if (originalTile.left.isEdge != generatedTile.left.isEdge) return false;

        if (originalTile.PlotInfo != generatedTile.PlotInfo) return false;

        return true;
    }

    private void GenerateTile(TileInfo tileInfo)
    {
        GameObject tile = tileInfo.PlotInfo.gameObject;

        // Remove all deco from tile
        foreach (Transform child in tileInfo.PlotInfo.gameObject.transform)
        {
            Destroy(child.gameObject);
        }

        // Generate edges
        GenerateEdges(tile, tileInfo);

        // Generate middle
        if (generateDeco)
        {
            GameObject deco = Instantiate(middlePrefab, tile.transform.position, Quaternion.Euler(0, Random.Range(0, 3) * 90, 0), tile.transform);
            CombineMesh(deco);
        }
    }

    private void GenerateEdges(GameObject tile, TileInfo tileInfo)
    {
        Vector3 tilePosition = tile.transform.position;
        float edgeDistance = 3.85f;

        List<(Vector3, Quaternion, GameObject)> edges = new();

        // Handle up
        if (tileInfo.up.isEdge && tileInfo.up.xLoc != -1 && tileInfo.up.yLoc != -1)
        {
            edges.Add((
                new Vector3(tilePosition.x - edgeDistance, tilePosition.y, tilePosition.z - 4),
                Quaternion.Euler(0, 90, 0),
                edgePrefab
            ));
        }
        else if (generateDeco)
        {
            edges.Add((
                new Vector3(tilePosition.x - edgeDistance, tilePosition.y, tilePosition.z),
                Quaternion.Euler(0, 0, 0),
                emptyEdgePrefab
            ));
        }

        // Handle down
        if (tileInfo.down.isEdge && tileInfo.down.xLoc != -1 && tileInfo.down.yLoc != -1)
        {
            edges.Add((
                new Vector3(tilePosition.x + edgeDistance, tilePosition.y, tilePosition.z + 4),
                Quaternion.Euler(0, 270, 0),
                edgePrefab
            ));
        }
        else if (generateDeco)
        {
            edges.Add((
                new Vector3(tilePosition.x + edgeDistance, tilePosition.y, tilePosition.z),
                Quaternion.Euler(0, 180, 0),
                emptyEdgePrefab
            ));
        }

        // Handle left
        if (tileInfo.left.isEdge && tileInfo.left.xLoc != -1 && tileInfo.left.yLoc != -1)
        {
            edges.Add((
                new Vector3(tilePosition.x + 4, tilePosition.y, tilePosition.z - edgeDistance),
                Quaternion.Euler(0, 0, 0),
                edgePrefab
            ));
        }
        else if (generateDeco)
        {
            edges.Add((
                new Vector3(tilePosition.x, tilePosition.y, tilePosition.z - edgeDistance),
                Quaternion.Euler(0, 90, 0),
                emptyEdgePrefab
            ));
        }

        // Handle right
        if (tileInfo.right.isEdge && tileInfo.right.xLoc != -1 && tileInfo.right.yLoc != -1)
        {
            edges.Add((
                new Vector3(tilePosition.x - 4, tilePosition.y, tilePosition.z + edgeDistance),
                Quaternion.Euler(0, 180, 0),
                edgePrefab
            ));
        }
        else if (generateDeco)
        {
            edges.Add((
                new Vector3(tilePosition.x, tilePosition.y, tilePosition.z + edgeDistance),
                Quaternion.Euler(0, 270, 0),
                emptyEdgePrefab
            ));
        }

        InitializeEdgePrefabs(edges, tile.transform);
    }

    private void InitializeEdgePrefabs(List<(Vector3, Quaternion, GameObject)> edges, Transform parent)
    {
        foreach (var edge in edges)
        {
            GameObject edgeObject = Instantiate(edge.Item3, edge.Item1, edge.Item2, parent);
            CombineMesh(edgeObject);
        }
    }

    private void CombineMesh(GameObject objectToCombine)
    {
        if (objectToCombine.name.Equals("EdgePrefab(Clone)")) return;

        Transform bamboo = objectToCombine.transform.Find("Bamboo");
        CombineSubmesh(bamboo, bambooMat);

        Transform rock = objectToCombine.transform.Find("Rocks");
        CombineSubmesh(rock, rockMat);
    }

    private void CombineSubmesh(Transform meshTransform, Material material)
    {
        MeshFilter[] meshFilters = meshTransform.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshTransform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        Mesh mesh = new()
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        mesh.CombineMeshes(combine);
        meshTransform.AddComponent<MeshRenderer>();
        meshTransform.AddComponent<MeshFilter>().sharedMesh = mesh;
        meshTransform.GetComponent<Renderer>().sharedMaterial = material;
        meshTransform.gameObject.SetActive(true);
    }
}
