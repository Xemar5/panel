//using Sirenix.OdinInspector;
//using Sirenix.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//[Serializable]
//public class Connection
//{
//    [Serializable]
//    public class List
//    {
//        public List<Connection> connections = new List<Connection>();
//    }
//    [Serializable]
//    public class Input
//    {
//        public int pieceIndex;
//        public int inputIndex;
//        public int closestOutputIndex;
//    }
//    [Serializable]
//    public class Output
//    {
//        public int pieceIndex;
//        public int outputIndex;
//    }

//    [SerializeField, ReadOnly]
//    private Output outputPiece;
//    [SerializeField, ReadOnly]
//    private List<Input> inputPieces = new List<Input>();

//    public Output OutputPiece { get => outputPiece; set => outputPiece = value; }
//    public List<Input> InputPieces => inputPieces;

//}
