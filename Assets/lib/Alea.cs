
using System;

// https://github.com/coverslide/node-alea/blob/master/alea.js
// http://baagoe.com/en/RandomMusings/javascript/
// https://github.com/dworthen/prng

namespace SRQ {
    public class AleaState {
        public readonly double[] data;
        public AleaState(double[] data) {
            this.data = data;
        }
    }

    public class Alea {
        private double s0 = 0;
        private double s1 = 0;
        private double s2 = 0;
        private int c = 1;

        public Alea(string args) {
            Mash mash = new Mash();
            this.s0 = mash.Compute(" ");
            this.s1 = mash.Compute(" ");
            this.s2 = mash.Compute(" ");

            for (int i = 0; i < args.Length; i++) {
                this.s0 -= mash.Compute(args[i].ToString());
                if (this.s0 < 0) {
                    this.s0 += 1;
                }
                this.s1 -= mash.Compute(args[i].ToString());
                if (this.s1 < 0) {
                    this.s1 += 1;
                }
                this.s2 -= mash.Compute(args[i].ToString());
                if (this.s2 < 0) {
                    this.s2 += 1;
                }
            }
        }

        public Alea(AleaState state) {
            ImportState(state);
        }

        public double Random(int? dec = null) {
            double t = 2091639 * this.s0 + this.c * 2.3283064365386963e-10;
            this.s0 = this.s1;
            this.s1 = this.s2;
            this.c = (int)t;
            this.s2 = t - this.c;
            double random = this.s2;
            return dec.HasValue ? Math.Floor(random * dec.Value) : random;
        }

        class Mash {
            private uint n = 0xefc8249d;

            public double Compute(string data) {
                unchecked {
                    for (int i = 0; i < data.Length; i++) {
                        n += data[i];
                        double h = 0.02519603282416938 * n;
                        n = (uint)h;
                        h -= n;
                        h *= n;
                        n = (uint)h;
                        h -= n;
                        n += (uint)(h * 0x100000000); // 2^32
                    }
                    return (n & 0xFFFFFFFF) * 2.3283064365386963e-10; // 2^-32
                }
            }
        }


        public AleaState ExportState() {
            return new AleaState(new double[] { this.s0, this.s1, this.s2, this.c });
        }

        public void ImportState(AleaState state) {
            this.s0 = state.data[0] != 0 ? state.data[0] : 0;
            this.s1 = state.data[1] != 0 ? state.data[1] : 0;
            this.s2 = state.data[2] != 0 ? state.data[2] : 0;
            this.c = state.data[3] != 0 ? (int)state.data[3] : 0;
        }
    }

}