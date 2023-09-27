using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Board : MonoBehaviour {

    public float timeBetweenPieces = 0.05f;
    public int width;
    public int height;
    public GameObject tileObject;
    public float cameraSizeOffset;
    public float cameraVerticalOffset;
    public GameObject[] availablePieces;

    Tile[,] Tiles;
    Piece[,] Pieces;

    Tile startTile;
    Tile endTile;
    bool swappingPieces = false;

    private void Start() {
        Tiles = new Tile[width, height];
        Pieces = new Piece[width, height];
        SetUpBoard();
        //PositionCamera();
        StartCoroutine(SetupPieces());
        
    }

    private IEnumerator SetupPieces() {

        int maxIterations = 50;
        int currentIteration = 0;
        
        for (int x = 0; x < width; x++) {
            currentIteration = 0;
            for (int y = 0; y < height; y++) {
                yield return new WaitForSeconds(timeBetweenPieces);
                if(Pieces[x, y] == null) {
                    currentIteration = 0;
                    //Pieza aleatorio dentro del arreglo piece
                    var newPiece = CreatePieceAt(x, y);
                    //Habia una pieza anterior?
                    while (HasPreviousMatches(x,y)) {
                    //Limpia y vuelve a crear una nueva pieza
                    ClearPieceAt(x,y);
                    newPiece = CreatePieceAt(x, y);
                    currentIteration++;
                    if (currentIteration > maxIterations) {
                        break;
                    }
                }
                }
            }
        }
        yield return null;
    }

    private void ClearPieceAt(int x, int y) {
        var pieceToClear = Pieces[x, y];
        Destroy(pieceToClear.gameObject);
        Pieces[x, y] = null;
    }

    private Piece CreatePieceAt(int x, int y) {
        var selectedPiece = availablePieces[UnityEngine.Random.Range(0,availablePieces.Length)];
        var o = Instantiate(selectedPiece, new Vector3(x, y, -5), Quaternion.identity);
        o.transform.parent = transform;
        //Encontrando el componente script Piece y usando su funcion
        Pieces[x, y] = o.GetComponent<Piece>();
        Pieces[x, y]?.Setup(x, y, this);
        return Pieces[x, y];
    }

    private void Update() {
        PositionCamera();
    }

    private void PositionCamera() {
        //Variables para el casting de la cammara
        float newPosX = (float)width / 2f;
        float newPosY = (float)height / 2f;

        //Accedemos a la camara main de Unity y moviendo su posicion (un offset de 0.5) 
        Camera.main.transform.position = new Vector3(newPosX - 0.5f, newPosY - 0.5f + cameraVerticalOffset, -10f);

        float horizontal = width + 1;
        float vertical = (height / 2) + 1;

        // Asiganando automaticamente el tamaño orthografico para la camara segun horizontal y vertical (ademas agrandar o reducir la camara)
        Camera.main.orthographicSize = horizontal > vertical ? horizontal + cameraSizeOffset : vertical + cameraSizeOffset;
    }

    private void SetUpBoard() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                var o = Instantiate(tileObject, new Vector3(x, y, -5), Quaternion.identity);
                o.transform.parent = transform;
                //Encontrando el componente script Tile y usando su funcion
                Tiles[x, y] = o.GetComponent<Tile>();
                Tiles[x, y]?.Setup(x, y, this);
            }
        }
    }

    public void TileDown(Tile tile_) {
        if(!swappingPieces) {
            startTile = tile_;
        }
    }

    public void TileOver(Tile tile_) {
        if(!swappingPieces) {
            endTile = tile_;
        }
    }

    public void TileUp(Tile tile_) {

        if(!swappingPieces) {
            if(startTile!=null && endTile != null && IsCloseTo(startTile, endTile)) {
                StartCoroutine(SwapTiles());
            }
        }
    }

    IEnumerator SwapTiles() {
        swappingPieces = true;
        var StartPiece = Pieces[startTile.x, startTile.y];
        var EndPiece = Pieces[endTile.x, endTile.y];

        StartPiece.Move(endTile.x, endTile.y);
        EndPiece.Move(startTile.x, startTile.y);

        Pieces[startTile.x, startTile.y] = EndPiece;
        Pieces[endTile.x, endTile.y] = StartPiece;

        //Esperando 0,6 segundos a que se cambien las piezas
        yield return new WaitForSeconds(0.6f);
        
        var startMatches = GetMatchByPiece(startTile.x, startTile.y, 3);
        var endMatches = GetMatchByPiece(endTile.x, endTile.y, 3);

        var allMatches = startMatches.Union(endMatches).ToList();

        if (allMatches.Count == 0) {
            StartPiece.Move(startTile.x, startTile.y);
            EndPiece.Move(endTile.x, endTile.y);

            Pieces[startTile.x, startTile.y] = StartPiece;
            Pieces[endTile.x, endTile.y] = EndPiece;
        }
        else {
            ClearPieces(allMatches);
        }

        startTile = null;
        endTile = null;
        swappingPieces = false;

        yield return null;
    }

    private void ClearPieces(List<Piece> piecesToClear) {
        piecesToClear.ForEach(piece => {
            ClearPieceAt(piece.x, piece.y);
        });

        List<int> columns = GetColumns(piecesToClear);
        List<Piece> collapsedPieces = CollapseColumns(columns, 0.3f);
        FindMatchsRecursively(collapsedPieces);
    }

    private void FindMatchsRecursively(List<Piece> collapsedPieces) {

        StartCoroutine(FindMatchsRecursivelyCoroutine(collapsedPieces));
    }

    IEnumerator FindMatchsRecursivelyCoroutine(List<Piece> collapsedPieces) {
        yield return new WaitForSeconds(1f);
        List<Piece> newMatches = new List<Piece>();
        collapsedPieces.ForEach(piece => {
            var matches = GetMatchByPiece(piece.x, piece.y, 3);
            if (matches != null) {
                newMatches = newMatches.Union(matches).ToList();
                ClearPieces(matches);
            }
        });
        if(newMatches.Count > 0) {
            var newCollapsedPieces = CollapseColumns(GetColumns(newMatches), 0.3f);
            FindMatchsRecursively(newCollapsedPieces);
        }
        else {
            StartCoroutine(SetupPieces());
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(SetupPieces());
        swappingPieces = false;
    }

    private List<int> GetColumns(List<Piece> piecesToClear) {
        var result = new List<int>();

        piecesToClear.ForEach(piece => {
            if (!result.Contains(piece.x))
            {
                result.Add(piece.x);
            }
        });
        return result;
    }

    private List<Piece> CollapseColumns(List<int> columns, float timeToCollapse) {
        List<Piece> movingPieces = new List<Piece>();

        for (int i = 0; i < columns.Count; i++) {
            var column = columns[i];
            for (int y = 0; y < height; y++) {
                if(Pieces[column, y] == null) {
                    for (int yplus = y + 1; yplus < height; yplus++) {
                        if (Pieces[column, yplus] != null)
                        {
                            Pieces[column, yplus].Move(column, y);
                            Pieces[column, y] = Pieces[column, yplus];
                            if(!movingPieces.Contains(Pieces[column, y])) {
                                movingPieces.Add(Pieces[column, y]);
                            }
                            Pieces[column, yplus] = null;
                            break;
                        }
                    }
                }
            }
        }
        return movingPieces;
    }

    public bool IsCloseTo(Tile start, Tile end) {
        // Verificando que el eje x este a 1 unidad separada y en eje y esten a la misma altura
        if(Math.Abs((start.x - end.x)) == 1 && start.y == end.y) {
            return true;
        }
        // Verificando que el eje y este a 1 unidad separada y en eje x esten a la misma altura
        if(Math.Abs((start.y - end.y))== 1 && start.x == end.x) {
            return true;
        }
        return false;
    }

    bool HasPreviousMatches(int posx, int posy) {
        var downMatches = GetMatchByDirection(posx, posy, new Vector2(0,-1), 2);
        var leftMatches = GetMatchByDirection(posx,posy, new Vector2(-1, 0), 2);

        if (downMatches == null) downMatches = new List<Piece>();
        if (leftMatches == null) leftMatches = new List<Piece>();

        return (downMatches.Count > 0 || leftMatches.Count > 0);
    }

    public List<Piece> GetMatchByDirection(int xpos, int ypos, Vector2 direction, int minPieces) {
        List<Piece> matches = new List<Piece>();
        Piece startPiece = Pieces[xpos, ypos];
        matches.Add(startPiece);

        int nextX;
        int nextY;
        int maxVal = width > height ? width : height;

        for(int i =1; i < maxVal; i++) {
            nextX = xpos + ((int)direction.x * i);
            nextY = ypos + ((int)direction.y * i);
            if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height) {
                var nextPiece = Pieces[nextX, nextY];
                if (nextPiece!= null && nextPiece.pieceType == startPiece.pieceType) {
                    matches.Add(nextPiece);
                }
                else {
                    break;
                }
            }
        }

        if (matches.Count >= minPieces) {
            return matches;
        }
        return null;
    }

    public List<Piece> GetMatchByPiece(int xpos, int ypos, int minPieces = 3) {
        var upMatchs = GetMatchByDirection(xpos, ypos, new Vector2(0, 1), 2);
        var downMatchs = GetMatchByDirection(xpos, ypos, new Vector2(0, -1), 2);
        var rightMatchs = GetMatchByDirection(xpos, ypos, new Vector2(1, 0), 2);
        var leftMatchs = GetMatchByDirection(xpos, ypos, new Vector2(-1, 0), 2);

        if (upMatchs == null) upMatchs = new List<Piece>();
        if (downMatchs == null) downMatchs = new List<Piece>();
        if (rightMatchs == null) rightMatchs = new List<Piece>();
        if (leftMatchs == null) leftMatchs = new List<Piece>();

        var verticalMatches = upMatchs.Union(downMatchs).ToList();
        var horizontalMatches = leftMatchs.Union(rightMatchs).ToList();

        var foundMatches = new List<Piece>();

        if(verticalMatches.Count >= minPieces) {
            foundMatches = foundMatches.Union(verticalMatches).ToList();
        }

        if (horizontalMatches.Count >= minPieces) {
            foundMatches = foundMatches.Union(horizontalMatches).ToList();
        }

        return foundMatches;

    }

}



