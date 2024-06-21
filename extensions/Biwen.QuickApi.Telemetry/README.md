### Biwen.QuickApi.Telemetry

用于监控API调用的性能和错误信息。

#### 使用方法

##### 引用 Biwen.QuickApi.Telemetry 2.0.0 版本。


##### 修改`Prometheus`服务的配置文件，添加以下配置

假定服务地址为`localhost:5101`，则添加以下配置：

```yaml
# A scrape configuration containing exactly one endpoint to scrape:
# Here it's Prometheus itself.
scrape_configs:
  # The job name is added as a label `job=<job_name>` to any timeseries scraped from this config.
  - job_name: "prometheus"

    # metrics_path defaults to '/metrics'
    # scheme defaults to 'http'.

    static_configs:
      - targets: ["localhost:5101"]
```

##### 在`grafana`控制面板中添加对应的数据源

假定`Prometheus`服务的地址为`localhost:9090`，则添加数据源的方法如下：

菜单: `链接` -> `数据源` -> `添加数据源` -> `Prometheus` -> 
`Connection`选项栏 -> 填写ServiceUrl `http://localhost:9090` -> `Save & Test`

##### 在`grafana`控制面板中导入`dashboard`

这里主要是查询数据然后Pin到面板中 不详细介绍,如果有不明白的可以查阅`grafana`帮助文档~
