const int VertIndexPerFaces[20][3] = {
    {0, 1, 7}, // Face 0
    {0, 4, 1}, // Face 1
    {0, 7, 9}, // Face 2
    {0, 8, 4}, // Face 3
    {0, 9, 8}, // Face 4
    {1, 11, 10}, // Face 5
    {1, 10, 7}, // Face 6
    {1, 4, 11}, // Face 7
    {2, 3, 6}, // Face 8
    {2, 5, 3}, // Face 9
    {2, 6, 10}, // Face 10
    {2, 10, 11}, // Face 11
    {2, 11, 5}, // Face 12
    {3, 5, 8}, // Face 13
    {3, 8, 9}, // Face 14
    {3, 9, 6}, // Face 15
    {4, 5, 11}, // Face 16
    {4, 8, 5}, // Face 17
    {6, 7, 10}, // Face 18
    {6, 9, 7}, // Face 19
};

const Vector3 Vertices[] = {
    {0, -0.5257311, -0.8506508},
    {0, 0.5257311, -0.8506508},
    {0, 0.5257311, 0.8506508},
    {0, -0.5257311, 0.8506508},
    {-0.8506508, 0, -0.5257311},
    {-0.8506508, 0, 0.5257311},
    {0.8506508, 0, 0.5257311},
    {0.8506508, 0, -0.5257311},
    {-0.5257311, -0.8506508, 0},
    {0.5257311, -0.8506508, 0},
    {0.5257311, 0.8506508, 0},
    {-0.5257311, 0.8506508, 0},
};

const Vector3 SegmentGroupTriList[20][3] = {
    {
        {-4.670862E-07, -0.525732, -0.850651},
        {-4.670862E-07, 0.525732, -0.850651},
        {0.8506518, 0, -0.5257308},
    },
    {
        {4.670862E-07, -0.525732, -0.850651},
        {-0.8506518, 0, -0.5257308},
        {4.670862E-07, 0.525732, -0.850651},
    },
    {
        {-7.557613E-07, -0.5257313, -0.8506515},
        {0.8506515, 7.557613E-07, -0.5257313},
        {0.5257313, -0.8506515, 7.557613E-07},
    },
    {
        {7.557613E-07, -0.5257313, -0.8506515},
        {-0.5257313, -0.8506515, 7.557613E-07},
        {-0.8506515, 7.557613E-07, -0.5257313},
    },
    {
        {0, -0.5257308, -0.8506518},
        {0.525732, -0.850651, 4.670862E-07},
        {-0.525732, -0.850651, 4.670862E-07},
    },
    {
        {0, 0.5257308, -0.8506518},
        {-0.525732, 0.850651, 4.670862E-07},
        {0.525732, 0.850651, 4.670862E-07},
    },
    {
        {-7.557613E-07, 0.5257313, -0.8506515},
        {0.5257313, 0.8506515, 7.557613E-07},
        {0.8506515, -7.557613E-07, -0.5257313},
    },
    {
        {7.557613E-07, 0.5257313, -0.8506515},
        {-0.8506515, -7.557613E-07, -0.5257313},
        {-0.5257313, 0.8506515, 7.557613E-07},
    },
    {
        {-4.670862E-07, 0.525732, 0.850651},
        {-4.670862E-07, -0.525732, 0.850651},
        {0.8506518, 0, 0.5257308},
    },
    {
        {4.670862E-07, 0.525732, 0.850651},
        {-0.8506518, 0, 0.5257308},
        {4.670862E-07, -0.525732, 0.850651},
    },
    {
        {-7.557613E-07, 0.5257313, 0.8506515},
        {0.8506515, -7.557613E-07, 0.5257313},
        {0.5257313, 0.8506515, -7.557613E-07},
    },
    {
        {0, 0.5257308, 0.8506518},
        {0.525732, 0.850651, -4.670862E-07},
        {-0.525732, 0.850651, -4.670862E-07},
    },
    {
        {7.557613E-07, 0.5257313, 0.8506515},
        {-0.5257313, 0.8506515, -7.557613E-07},
        {-0.8506515, -7.557613E-07, 0.5257313},
    },
    {
        {7.557613E-07, -0.5257313, 0.8506515},
        {-0.8506515, 7.557613E-07, 0.5257313},
        {-0.5257313, -0.8506515, -7.557613E-07},
    },
    {
        {0, -0.5257308, 0.8506518},
        {-0.525732, -0.850651, -4.670862E-07},
        {0.525732, -0.850651, -4.670862E-07},
    },
    {
        {-7.557613E-07, -0.5257313, 0.8506515},
        {0.5257313, -0.8506515, -7.557613E-07},
        {0.8506515, 7.557613E-07, 0.5257313},
    },
    {
        {-0.850651, -4.670862E-07, -0.525732},
        {-0.850651, -4.670862E-07, 0.525732},
        {-0.5257308, 0.8506518, 0},
    },
    {
        {-0.850651, 4.670862E-07, -0.525732},
        {-0.5257308, -0.8506518, 0},
        {-0.850651, 4.670862E-07, 0.525732},
    },
    {
        {0.850651, -4.670862E-07, 0.525732},
        {0.850651, -4.670862E-07, -0.525732},
        {0.5257308, 0.8506518, 0},
    },
    {
        {0.850651, 4.670862E-07, 0.525732},
        {0.5257308, -0.8506518, 0},
        {0.850651, 4.670862E-07, -0.525732},
    },
};

const AxisOrientation FaceAxisOrientationList[20] = {
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
    AxisOrientation_CW,
};

const NeighborInfo NeighborFaceInfoList[20][3] = {
    {
        {6, EdgeNeighbor_O, EdgeNeighborOrigin_A, AxisOrientation_CW},
        {2, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {1, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {7, EdgeNeighbor_O, EdgeNeighborOrigin_B, AxisOrientation_CW},
        {0, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {3, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {19, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {4, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {0, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {17, EdgeNeighbor_O, EdgeNeighborOrigin_B, AxisOrientation_CW},
        {1, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {4, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {14, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {3, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {2, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {11, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {6, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {7, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {18, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {0, EdgeNeighbor_A, EdgeNeighborOrigin_Ap, AxisOrientation_CW},
        {5, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {16, EdgeNeighbor_O, EdgeNeighborOrigin_A, AxisOrientation_CW},
        {5, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {1, EdgeNeighbor_B, EdgeNeighborOrigin_Bp, AxisOrientation_CW},
    },
    {
        {15, EdgeNeighbor_O, EdgeNeighborOrigin_A, AxisOrientation_CW},
        {10, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {9, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {13, EdgeNeighbor_O, EdgeNeighborOrigin_B, AxisOrientation_CW},
        {8, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {12, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {18, EdgeNeighbor_O, EdgeNeighborOrigin_A, AxisOrientation_CW},
        {11, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {8, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {5, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {12, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {10, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {16, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {9, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {11, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {17, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {14, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {9, EdgeNeighbor_B, EdgeNeighborOrigin_Bp, AxisOrientation_CW},
    },
    {
        {4, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {15, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {13, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {19, EdgeNeighbor_O, EdgeNeighborOrigin_B, AxisOrientation_CW},
        {8, EdgeNeighbor_A, EdgeNeighborOrigin_Ap, AxisOrientation_CW},
        {14, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {12, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {7, EdgeNeighbor_A, EdgeNeighborOrigin_Ap, AxisOrientation_CW},
        {17, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {13, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {16, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {3, EdgeNeighbor_B, EdgeNeighborOrigin_Bp, AxisOrientation_CW},
    },
    {
        {6, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {10, EdgeNeighbor_A, EdgeNeighborOrigin_Ap, AxisOrientation_CW},
        {19, EdgeNeighbor_B, EdgeNeighborOrigin_O, AxisOrientation_CW},
    },
    {
        {2, EdgeNeighbor_O, EdgeNeighborOrigin_Op, AxisOrientation_CW},
        {18, EdgeNeighbor_A, EdgeNeighborOrigin_O, AxisOrientation_CW},
        {15, EdgeNeighbor_B, EdgeNeighborOrigin_Bp, AxisOrientation_CW},
    },
};
