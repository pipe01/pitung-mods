using PiTung.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace ShareMod.UI
{
    internal class UploadWorldUI : LoginUI
    {
        private bool Uploading = false;

        public UploadWorldUI(Remote remote) : base(remote)
        {
        }

        public override void Draw()
        {
            if (!(RunMainMenu.Instance.LoadGameCanvas?.enabled ?? false) || !Remote.User.IsLoggedIn)
                return;

            GUI.enabled = LoadGameMenu.Instance.SelectedSaveFile != null && !Uploading;
            if (GUI.Button(new Rect(2, 37, 150, 55), "Upload world"))
            {
                ShareMod.PlayButtonSound();

                UploadWorld(Application.persistentDataPath + "/saves/" + LoadGameMenu.Instance.SelectedSaveFile.FileName);
            }
        }

        private void UploadWorld(string folderPath)
        {
            new Thread(() =>
            {
                Uploading = true;

                byte[] b = Zipper.CompressFolder(folderPath);

                var result = Remote.UploadWorld(b, Path.GetFileName(folderPath));

                Uploading = false;

                if (result.Error == null)
                {
                    MessageBox.Show("Your world has been uploaded!", "Success");
                }
                else
                {
                    MessageBox.Show("Error while uploading world: " + result.Error, "Error");
                }
            }).Start();
        }
    }
}
