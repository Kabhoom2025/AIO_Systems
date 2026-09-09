namespace Platform.Domain.Enums;

public enum ComponentType
{
    Text,
    Input,
    Number,
    Email,
    Select,
    Checkbox,
    Radio,
    Date,
    Button,
    Table,
    Card,
    Form,
    Label,
    Container,
}

public enum ColumnDataType
{
    Uuid,
    Varchar,
    Text,
    Integer,
    Bigint,
    Decimal,
    Boolean,
    Date,
    Timestamp,
    Jsonb,
}

public enum ApiHttpMethod
{
    Get,
    Post,
    Put,
    Patch,
    Delete,
}

public enum TransformationType
{
    None,
    Trim,
    Uppercase,
    Lowercase,
    Default,
    Concatenate,
    Split,
    DateConversion,
    NumberConversion,
}

public enum ExecutionStatus
{
    Pending,
    Running,
    Success,
    Failed,
    Skipped,
}

public enum LineageNodeType
{
    Screen,
    Component,
    Api,
    Controller,
    Service,
    Repository,
    Database,
    Table,
    Column,
    Transformation,
}

public enum LineageEventType
{
    ScreenStarted,
    ComponentStarted,
    ApiStarted,
    ApiCompleted,
    ApiFailed,
    ServiceStarted,
    ServiceCompleted,
    ServiceFailed,
    DatabaseStarted,
    DatabaseCompleted,
    DatabaseFailed,
    TransformationStarted,
    TransformationCompleted,
    ValidationFailed,
    ExecutionCompleted,
}
