﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

abstract class Opponent
{
    abstract public int GetMove(ConnectFour game, int player);
}