# Aden Bot

## 对话流程

```flow
st=>start: 开始对话
input=>inputoutput: 输入问题
cond=>condition: 是否查询文件服务器
searchOp=>operation: 搜索匹配的文件信息返回
qnaOp=>operation: 从 QnA Maker 中获取解决方案
cond2=>condition: 是否找到对应的解决方案
opSolution=>operation: 自动发起请求
opSolution2=>subroutine: 延时任务（15分钟后自动改变请求状态）
opRequest=>operation: 手动发起请求
result=>inputoutput: 输出结果
e=>end: 结束对话

st(left)->input->cond
cond(no,left)->qnaOp->cond2(yes)
cond(yes)->searchOp->result
cond2(no,left)->opRequest->result
cond2(yes,right)->opSolution->opSolution2->result
result(right)->e
```

## 运行
```
dotnet restore
dotnet run 
```

## 发布
```
dotnet publish -c Release -o publish
```