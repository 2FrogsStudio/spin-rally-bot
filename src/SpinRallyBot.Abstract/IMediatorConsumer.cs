using MassTransit;

namespace SpinRallyBot;

public interface IMediatorConsumer<in T> : IConsumer<T> where T : class { }