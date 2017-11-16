using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Web.OfficeAzureCommunication {
    class ServerListStateChangedController {
        StateChangedController stateChangedController;
        Action<IEnumerable<WorkSessionServerInfo>> serversAdded;
        Action<IEnumerable<WorkSessionServerInfo>> serversRemoved;

        public ServerListStateChangedController(
            Func<object> getStateDelegate,
            Action<IEnumerable<WorkSessionServerInfo>> serversAdded,
            Action<IEnumerable<WorkSessionServerInfo>> serversRemoved) {

            this.serversAdded = serversAdded;
            this.serversRemoved = serversRemoved;
            this.stateChangedController = new StateChangedController(getStateDelegate, CheckStateChanged);
        }

        void CheckStateChanged(object stateBeforeChanges, object stateAfterChanges) {
            var serversBeforeChanges = (IEnumerable<WorkSessionServerInfo>)stateBeforeChanges;
            var serversAfterChanges = (IEnumerable<WorkSessionServerInfo>)stateAfterChanges;

            var added = serversAfterChanges.Except(serversBeforeChanges, new WorkSessionServerInfoComparer());
            var removed = serversBeforeChanges.Except(serversAfterChanges, new WorkSessionServerInfoComparer());

            if(added.Count() > 0)
                this.serversAdded(added);

            if(removed.Count() > 0)
                this.serversRemoved(removed);

        }

        public void BeginUpdate() {
            stateChangedController.BeginUpdate();
        }
        public void EndUpdate() {
            stateChangedController.EndUpdate();
        }
    }
    class StateChangedController {
        object stateBeforeChanges = null;
        object stateAfterChanges = null;

        Func<object> getStateDelegate;
        Action<object, object> checkStateChanged;

        public StateChangedController(
            Func<object> getStateDelegate,
            Action<object, object> checkStateChanged) {

            this.getStateDelegate = getStateDelegate;
            this.checkStateChanged = checkStateChanged;
        }

        int counter = 0;
        public void BeginUpdate() {
            counter++;
            if(counter == 1)
                stateBeforeChanges = getStateDelegate();
        }
        public void EndUpdate() {
            counter--;
            if(counter == 0 && stateBeforeChanges != null) {
                stateAfterChanges = getStateDelegate();
                checkStateChanged(stateBeforeChanges, stateAfterChanges);
                stateBeforeChanges = null;
                stateAfterChanges = null;
            }

            if(counter < 0)
                counter = 0;
        }
    }

}
