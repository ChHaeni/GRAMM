﻿#region Copyright
///<remarks>
/// <GRAMM Mesoscale Model>
/// Copyright (C) [2019]  [Dietmar Oettl, Markus Kuntner]
/// This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
/// the Free Software Foundation version 3 of the License
/// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
/// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
///</remarks>
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices; 

namespace GRAMM_CSharp_Test
{
    partial class Program
    {
        // procedure calculating the diffusion and advection terms for the implicit scheme (Patankar 1980, p52)
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void TERMIPSterms(int NI, int NJ, int NK)
        {
          Parallel.For(2, NI, Program.pOptions, i =>
          {
        	    double DEAST, DWEST, DSOUTH, DNORTH, DBOTTOM, DTOP, FEAST, FWEST, FSOUTH, FNORTH, FBOTTOM, FTOP;
                double PEAST, PWEST, PSOUTH, PNORTH, PBOTTOM, PTOP;
                double VISH1, AREA;
				float RHO_I1, RHO_IM1, RHO_J1, RHO_JM1;                    
                double DDX_Rez = 1 / Program.DDX[i];
                float RHO;
          
                for (int j = 2; j <= NJ-1; j++)
                {
                	float[] AREA_L	     = Program.AREA[i][j];
                    float[] AREAX_L      = Program.AREAX[i][j];
                    float[] AREAY_L      = Program.AREAY[i][j];
                    float[] AREAZX_L     = Program.AREAZX[i][j];
                    float[] AREAZY_L     = Program.AREAZY[i][j];
                    float[] AEAST_PS_L   = Program.AEAST_PS[i][j];
                    float[] ANORTH_PS_L  = Program.ANORTH_PS[i][j];
                    float[] ASOUTH_PS_L  = Program.ASOUTH_PS[i][j];
                    float[] AWEST_PS_L   = Program.AWEST_PS[i][j];
                    float[] AP0_PS_L     = Program.AP0_PS[i][j];
                    double[] A_PS_L      = Program.A_PS[i][j];
                    double[] B_PS_L      = Program.B_PS[i][j];
                    double[] C_PS_L      = Program.C_PS[i][j];
                    float[] RHO_L      = Program.RHO[i][j];
                    double[] UN_L        = Program.UN[i][j];
                    double[] VN_L        = Program.VN[i][j];
                    double[] WN_L        = Program.WN[i][j];
                    float[]  ZSP_L       = Program.ZSP[i][j];
                    double[] VISH_L      = Program.VISH[i][j];
                    float[]  VOL_L		 = Program.VOL[i][j];
                    
                    double DDY_Rez = 1 / Program.DDY[j];
					
                    for (int k = 1; k <= NK; k++)
                    {
                         DEAST = DWEST = DSOUTH = DNORTH = DBOTTOM = DTOP = 0;
						 FEAST = FWEST = FSOUTH = FNORTH = FBOTTOM = FTOP = 0;
						 PEAST = PWEST = PSOUTH = PNORTH = PBOTTOM = PTOP = 0;
                        
                         RHO   = RHO_L[k];
                         VISH1 = VISH_L[k];
                         AREA  = AREA_L[k];

                         RHO_I1 = Program.RHO[i + 1][j][k];  RHO_IM1 = Program.RHO[i - 1][j][k];
                         RHO_J1 = Program.RHO[i][j + 1][k];  RHO_JM1 = Program.RHO[i][j - 1][k];
                        
                        //DIFFUSION TERMS (NOTE THAT VIS IS COMPUTED AS HARMONIC MEAN RATHER THAN ARITHMETIC MEAN AT BORDERS ACCORDING TO PATANKAR 1980, CHAP. 4.2.3)
                        DEAST = 0.5F * (VISH1 + Program.VISH[i + 1][j][k]) * Program.AREAX[i + 1][j][k] * DDX_Rez * 0.5F * (RHO + RHO_I1);
                        DWEST = 0.5F * (VISH1 + Program.VISH[i - 1][j][k]) * AREAX_L[k] * DDX_Rez * 0.5F * (RHO + RHO_IM1);
                        DNORTH = 0.5F * (VISH1 + Program.VISH[i][j + 1][k]) * Program.AREAY[i][j + 1][k] * DDY_Rez * 0.5F * (RHO + RHO_J1);
                        DSOUTH = 0.5F * (VISH1 + Program.VISH[i][j - 1][k]) * AREAY_L[k] * DDY_Rez * 0.5F * (RHO + RHO_JM1);
                        if (k > 1)
                            DBOTTOM = (0.5F * (VISH1 + VISH_L[k - 1]) * (AREA / (ZSP_L[k] - ZSP_L[k - 1]) +
                                AREAZX_L[k] * DDX_Rez + AREAZY_L[k] * DDY_Rez)) * 0.5F * (RHO + RHO_L[k - 1]);
                        if (k < NK)
                            DTOP = (0.5F * (VISH1 + VISH_L[k + 1]) * (AREA / (ZSP_L[k + 1] - ZSP_L[k]) +
                                AREAZX_L[k + 1] * DDX_Rez + AREAZY_L[k + 1] * DDY_Rez)) * 0.5F * (RHO + RHO_L[k + 1]);

                        //Advection terms
                        FEAST = 0.5F * (Program.UN[i + 1][j][k] + UN_L[k]) * Program.AREAX[i + 1][j][k] * 0.5F * (RHO + RHO_I1);
                        FWEST = 0.5F * (Program.UN[i - 1][j][k] + UN_L[k]) * AREAX_L[k] * 0.5F * (RHO + RHO_IM1);
                        FNORTH = 0.5F * (Program.VN[i][j + 1][k] + VN_L[k]) * Program.AREAY[i][j + 1][k] * 0.5F * (RHO + RHO_J1);
                        FSOUTH = 0.5F * (Program.VN[i][j - 1][k] + VN_L[k]) * AREAY_L[k] * 0.5F * (RHO + RHO_JM1);
                        if (k > 1)
                            FBOTTOM = (0.5F * (WN_L[k - 1] + WN_L[k]) * AREA
                                    + 0.5F * (UN_L[k - 1] + UN_L[k]) * AREAZX_L[k]
                                    + 0.5F * (VN_L[k - 1] + VN_L[k]) * AREAZY_L[k]) *
                                    0.5F * (RHO + RHO_L[k - 1]);
                        if (k < NK)
                            FTOP = (0.5F * (WN_L[k + 1] + WN_L[k]) * AREA
                                    + 0.5F * (UN_L[k + 1] + UN_L[k]) * AREAZX_L[k + 1]
                                    + 0.5F * (VN_L[k + 1] + VN_L[k]) * AREAZY_L[k + 1]) *
                                    0.5F * (RHO + RHO_L[k + 1]);

                        //Peclet numbers
                        DEAST = Math.Max(DEAST, 0.0001F);
                        DWEST = Math.Max(DWEST, 0.0001F);
                        DSOUTH = Math.Max(DSOUTH, 0.0001F);
                        DNORTH = Math.Max(DNORTH, 0.0001F);
                        DBOTTOM = Math.Max(DBOTTOM, 0.0001F);
                        DTOP = Math.Max(DTOP, 0.0001F);
                        PEAST = Math.Abs(FEAST / DEAST);
                        PWEST = Math.Abs(FWEST / DWEST);
                        PSOUTH = Math.Abs(FSOUTH / DSOUTH);
                        PNORTH = Math.Abs(FNORTH / DNORTH);
                        PBOTTOM = Math.Abs(FBOTTOM / DBOTTOM);
                        PTOP = Math.Abs(FTOP / DTOP);

                        //calculate coefficients of source terms
                        /*double SMP = (FEAST - FWEST + FNORTH - FSOUTH - FBOTTOM + FTOP) * 0;
                        double CPI = Math.Max(0, SMP);
                        double SP = -CPI;
                        Program.SU_PS[i][j][k] = CPI;
                         */

                        //advection scheme "power-law" by Patankar 1980, p90
                        B_PS_L[k] = DTOP * Math.Max(0, Pow5(1 - 0.1F * PTOP)) + Math.Max(-FTOP, 0);
                        C_PS_L[k] = DBOTTOM * Math.Max(0, Pow5(1 - 0.1F * PBOTTOM)) + Math.Max(FBOTTOM, 0);
                        AWEST_PS_L[k]  = (float) (DWEST * Math.Max(0, Pow5(1 - 0.1F * PWEST)) + Math.Max(FWEST, 0));
                        ASOUTH_PS_L[k] = (float) (DSOUTH * Math.Max(0, Pow5(1 - 0.1F * PSOUTH)) + Math.Max(FSOUTH, 0));
                        AEAST_PS_L[k]  = (float) (DEAST * Math.Max(0, Pow5(1 - 0.1F * PEAST)) + Math.Max(-FEAST, 0));
                        ANORTH_PS_L[k] = (float) (DNORTH * Math.Max(0, Pow5(1 - 0.1F * PNORTH)) + Math.Max(-FNORTH, 0));

                        AP0_PS_L[k] = (float) (VOL_L[k] / Program.DT * RHO);
                        A_PS_L[k] = B_PS_L[k] + C_PS_L[k] + AWEST_PS_L[k] + ASOUTH_PS_L[k]
                            + ANORTH_PS_L[k] + AEAST_PS_L[k] + AP0_PS_L[k];  // -SP;
                    }
                 }
       
            });
        }
    }
}