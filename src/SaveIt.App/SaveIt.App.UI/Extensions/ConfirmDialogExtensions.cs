using BlazorBootstrap;
using SaveIt.App.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SaveIt.App.UI.Components;
public static class ConfirmDialogExtensions
{
    public static async Task<bool> ShowDeleteDialogAsync(this ConfirmDialog dialog, string title, string message1,
        string? message2 = null)
    {
        var options = new ConfirmDialogOptions
        {
            YesButtonText = "Delete",
            YesButtonColor = ButtonColor.Primary,
            NoButtonText = "Cancel",
            NoButtonColor = ButtonColor.Light,
            Size = DialogSize.Large,
            IsVerticallyCentered = true,
            DialogCssClass = "fs-5"
        };

        var confirmation = await dialog.ShowAsync(
            title: title,
            message1: message1,
            message2: message2!,
            options);

        return confirmation;
    }
}
