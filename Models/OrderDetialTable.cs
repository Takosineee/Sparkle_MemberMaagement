﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Gym.Models;

public partial class OrderDetialTable
{
    public int Id { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? MemberId { get; set; }

    public int? ProductCapacity { get; set; }

    public virtual OrderTable Order { get; set; }

    public virtual ProductsTable Product { get; set; }
}