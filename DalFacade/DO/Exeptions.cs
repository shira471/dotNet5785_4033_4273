namespace DO;
//Exception thrown when attempting to access an entity that does not exist in the data source.
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message) { }
}
//Exception thrown when attempting to add an entity that already exists in the data source.
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException(string? message) : base(message) { }
}
//Exception thrown when an entity cannot be deleted from the data source.
public class DalDeletionImpossibleException : Exception
{
    public DalDeletionImpossibleException(string? message) : base(message) { }
}
//Exception thrown when an invalid phone number is provided for an entity.
public class DalImposiblePhoneNumber : Exception
{
    public DalImposiblePhoneNumber(string? message) : base(message) { }
}
