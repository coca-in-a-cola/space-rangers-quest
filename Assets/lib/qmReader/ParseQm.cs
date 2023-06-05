using SRQ;
using System;
using System.Collections.Generic;

namespace SRQ {
    public partial class QM {
        public static QM Parse(byte[] data) {
            Reader r = new Reader(data);
            int header = r.Int32();

            QM qmBase = QM.ParseBase(r, header);

            bool isQmm = header == Constants.HEADER_QMM_6
                || header == Constants.HEADER_QMM_7
                || header == Constants.HEADER_QMM_7_WITH_OLD_TGE_BEHAVIOUR;

            List<QMParam> qmParams = new List<QMParam>();
            for (int i = 0; i < qmBase.ParamsCount; i++) {
                qmParams.Add(isQmm ? QMParam.ParseParamQmm(r) : QMParam.ParseParam(r));
            }

            QM.ParseBase2(qmBase, r, isQmm);

            List<Location> locations = new List<Location>();
            for (int i = 0; i < qmBase.LocationsCount; i++) {
                locations.Add(isQmm ? Location.ParseLocationQmm(r, qmBase.ParamsCount) : Location.ParseLocation(r, qmBase.ParamsCount));
            }

            List<Jump> jumps = new List<Jump>();
            for (int i = 0; i < qmBase.JumpsCount; i++) {
                jumps.Add(isQmm ? Jump.ParseJumpQmm(r, qmBase.ParamsCount, qmParams) : Jump.ParseJump(r, qmBase.ParamsCount));
            }

            if (r.IsNotEnd() != null) {
                throw new Exception("Not at end of stream");
            }

            qmBase.Header = header;
            qmBase.QMParams = qmParams;
            qmBase.Locations = locations;
            qmBase.Jumps = jumps;

            return qmBase;
        }
    }
}