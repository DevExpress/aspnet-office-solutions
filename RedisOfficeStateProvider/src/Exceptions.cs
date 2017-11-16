using System;

namespace DevExpress.Web.RedisOfficeStateProvider {

    public class CannotCheckoutStateCheckedOutByAnotherProcessException : Exception {
        const string message = "Cannot check out a state that has already been checked out by another process.";
        public CannotCheckoutStateCheckedOutByAnotherProcessException() : base(message) { }
    }

    public class CannotAddStateCheckedOutByAnotherProcessException : Exception {
        const string message = "Cannot add a record for the state that has already been checked out by another process.";
        public CannotAddStateCheckedOutByAnotherProcessException() : base(message) { }
    }

    public class CannotRemoveStateCheckedOutByAnotherProcessException : Exception {
        const string message = "Cannot remove a record for the state that has already been checked out by another process.";
        public CannotRemoveStateCheckedOutByAnotherProcessException() : base(message) { }
    }

    public class CannotRemoveStateException : Exception {
        const string message = "Cannot remove a state record.";
        public CannotRemoveStateException(string field) : base(string.Format(message, field)) { }
    }

    public interface IDoNotRetryRedisOfficeException { }

    public class CannotAddWorkSessionThatAlreadyExistsException : Office.Internal.CannotAddWorkSessionThatAlreadyExistsException, IDoNotRetryRedisOfficeException { 
    }

}
