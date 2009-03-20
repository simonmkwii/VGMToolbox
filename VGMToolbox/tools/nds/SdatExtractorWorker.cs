﻿using System;
using System.ComponentModel;
using System.IO;

using VGMToolbox.format.util;
using VGMToolbox.util;

namespace VGMToolbox.tools.nds
{
    class SdatExtractorWorker : BackgroundWorker
    {
        private int fileCount = 0;
        private int maxFiles = 0;
        private Constants.ProgressStruct progressStruct;

        public struct SdatExtractorStruct
        {
            public string[] pPaths;
            public int totalFiles;
        }

        public SdatExtractorWorker()
        {
            fileCount = 0;
            maxFiles = 0;
            progressStruct = new Constants.ProgressStruct();
            
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;
        }

        private void extractSdats(SdatExtractorStruct pSdatExtractorStruct, 
            DoWorkEventArgs e)
        {
            foreach (string path in pSdatExtractorStruct.pPaths)
            {                
                if (File.Exists(path))
                {
                    if (!CancellationPending)
                    {
                        this.extractSdatFromFile(path, e);
                    }
                    else 
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else if (Directory.Exists(path))
                {
                    this.extractSdatsFromDirectory(path, e);

                    if (CancellationPending)
                    {
                        e.Cancel = true;
                        return;                        
                    }
                }                               
            }

            return;
        }

        private void extractSdatsFromDirectory(string pPath, DoWorkEventArgs e)
        {
            foreach (string d in Directory.GetDirectories(pPath))
            {
                if (!CancellationPending)
                {
                    this.extractSdatsFromDirectory(d, e);
                }
                else
                {
                    e.Cancel = true;
                    break;
                }
            }
            foreach (string f in Directory.GetFiles(pPath))
            {
                if (!CancellationPending)
                {
                    this.extractSdatFromFile(f, e);
                }
                else
                {
                    e.Cancel = true;
                    break;
                }
            }                
        }

        private void extractSdatFromFile(string pPath, DoWorkEventArgs e)
        {
            // Report Progress
            int progress = (++fileCount * 100) / maxFiles;
            this.progressStruct.Clear();
            this.progressStruct.filename = pPath;
            ReportProgress(progress, this.progressStruct);
         
            try
            {
                string outputDir = SdatUtil.ExtractSdat(pPath);
            }
            catch (Exception ex)
            {
                this.progressStruct.Clear();
                this.progressStruct.errorMessage = String.Format("Error processing <{0}>.  Error received: ", pPath) + ex.Message;
                ReportProgress(progress, this.progressStruct);
            }            
        }    

        protected override void OnDoWork(DoWorkEventArgs e)
        {
            SdatExtractorStruct sdatExtractorStruct = (SdatExtractorStruct)e.Argument;
            maxFiles = sdatExtractorStruct.totalFiles;

            this.extractSdats(sdatExtractorStruct, e);
        }    
    }
}
